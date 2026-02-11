using System.Reflection;
using System.Text.Json;
using Corsinvest.ProxmoxVE.Admin.Core.Components.WidgetGrid;

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Components;

internal class DashboardWidgetItem : WidgetGridItem
{
    public Widget Widget { get; }
    public IRefreshableData? Instance { get; private set; }

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ModuleWidget? _moduleWidget;

    private DashboardWidgetItem(Widget widget, IServiceScopeFactory serviceScopeFactory)
    {
        Widget = widget;
        _serviceScopeFactory = serviceScopeFactory;

        using var scope = _serviceScopeFactory.CreateScope();
        _moduleWidget = Widget.GetModuleWidget(scope.GetRequiredService<IModuleService>());
    }

    private static readonly MethodInfo? _eventCallbackCreateMethod = typeof(EventCallbackFactory)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(m => m.Name == "Create"
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[0].ParameterType == typeof(object)
                            && m.GetParameters()[1].ParameterType.IsGenericType
                            && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

    public static DashboardWidgetItem Create(Widget widget,
                                             IServiceScopeFactory serviceScopeFactory,
                                             IEnumerable<string> clusterNames,
                                             bool inEditing,
                                             object receiver)
    {
        var item = new DashboardWidgetItem(widget, serviceScopeFactory)
        {
            Id = widget.Id,
            Col = widget.X,
            Row = widget.Y,
            ColSpan = widget.Width,
            RowSpan = widget.Height,
            Title = widget.Title,
            TitleCss = widget.TitleCss,
            BodyCss = widget.BodyCss
        };

        var parameters = new Dictionary<string, object>();

        if (item._moduleWidget?.RenderInfo?.Type != null)
        {
            var renderType = item._moduleWidget.RenderInfo.Type;

            if (renderType.GetProperty(nameof(IModuleWidget<>.ClusterNames)) != null)
            {
                parameters[nameof(IModuleWidget<>.ClusterNames)] = clusterNames;
            }

            if (renderType.GetProperty(nameof(IModuleWidget<>.InEditing)) != null)
            {
                parameters[nameof(IModuleWidget<>.InEditing)] = inEditing;
            }

            if (item._moduleWidget.RenderSettingsInfo != null)
            {
                var settingsType = item._moduleWidget.RenderSettingsInfo.Type;
                var settings = DeserializeSettings(widget.SettingsJson, settingsType);

                if (settings != null)
                {
                    parameters[nameof(ISettingsParameter<>.Settings)] = settings;

                    var settingsChangedCallback = CreateSettingsChangedCallback(widget, settingsType, receiver);
                    if (settingsChangedCallback != null)
                    {
                        parameters[nameof(ISettingsParameter<>.SettingsChanged)] = settingsChangedCallback;
                    }
                }
            }

            if (item._moduleWidget.RenderInfo.Parameters != null)
            {
                foreach (var p in item._moduleWidget.RenderInfo.Parameters)
                {
                    parameters[p.Key] = p.Value;
                }
            }
        }

        var componentType = item._moduleWidget?.RenderInfo?.Type;
        if (componentType != null)
        {
            item.Template = builder =>
            {
                var seq = 0;
                builder.OpenComponent(seq++, componentType);
                foreach (var (key, value) in parameters) { builder.AddAttribute(seq++, key, value); }

                builder.AddComponentReferenceCapture(seq, inst =>
                {
                    if (inst is IRefreshableData refreshable) { item.Instance = refreshable; }
                });
                builder.CloseComponent();
            };
        }

        return item;
    }

    public async Task<bool> ConfigureAsync(DialogService dialogService)
    {
        var settings = _moduleWidget?.RenderSettingsInfo == null
                        ? null
                        : DeserializeSettings(Widget.SettingsJson, _moduleWidget.RenderSettingsInfo.Type);

        var model = new WidgetEdit(JsonSerializer.Deserialize<Widget>(JsonSerializer.Serialize(Widget))!,
                                   settings,
                                   _moduleWidget?.RenderSettingsInfo?.Render!);

        using var scope = _serviceScopeFactory.CreateScope();
        var localizer = scope.GetRequiredService<IStringLocalizer<DashboardWidgetItem>>();

        if (await dialogService.OpenSideEditAsync<WidgetDialog>(localizer["Configure widget"], false, model) != null)
        {
            Widget.Title = model.Widget.Title;
            Widget.TitleCss = model.Widget.TitleCss;
            Widget.BodyCss = model.Widget.BodyCss;
            Widget.SettingsJson = model.Settings == null ? null! : JsonSerializer.Serialize(model.Settings);
            return true;
        }
        return false;
    }

    private static object? DeserializeSettings(string? json, Type settingsType)
    {
        object? settings = null;
        if (!string.IsNullOrEmpty(json))
        {
            try { settings = JsonSerializer.Deserialize(json, settingsType); }
            catch { }
        }
        return settings ?? Activator.CreateInstance(settingsType);
    }

    private static object? CreateSettingsChangedCallback(Widget widget, Type settingsType, object receiver)
    {
        if (_eventCallbackCreateMethod == null) { return null; }

        try
        {
            Action<object> action = settings => widget.SettingsJson = JsonSerializer.Serialize(settings, settingsType);

            var actionType = typeof(Action<>).MakeGenericType(settingsType);
            var param = System.Linq.Expressions.Expression.Parameter(settingsType, "settings");
            var convertedParam = System.Linq.Expressions.Expression.Convert(param, typeof(object));
            var actionConst = System.Linq.Expressions.Expression.Constant(action);
            var invokeCall = System.Linq.Expressions.Expression.Invoke(actionConst, convertedParam);
            var lambda = System.Linq.Expressions.Expression.Lambda(actionType, invokeCall, param);
            var compiledDelegate = lambda.Compile();

            var genericMethod = _eventCallbackCreateMethod.MakeGenericMethod(settingsType);
            return genericMethod.Invoke(EventCallback.Factory, [receiver, compiledDelegate]);
        }
        catch
        {
            return null;
        }
    }
}
