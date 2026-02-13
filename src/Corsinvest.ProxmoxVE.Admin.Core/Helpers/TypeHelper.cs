/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class TypeHelper
{
    // public static List<T> GetInstances<T>(IServiceProvider serviceProvider)
    // {
    //     var type = typeof(T);

    //     var types = serviceProvider.GetRequiredService<IModuleService>().Assemblies
    //                 .SelectMany(s =>
    //                 {
    //                     try
    //                     {
    //                         return s.GetExportedTypes();
    //                     }
    //                     catch (ReflectionTypeLoadException ex)
    //                     {
    //                         return ex.Types.Where(t => t != null)!;
    //                     }
    //                 })
    //                 .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
    //                 .ToList();

    //     return [.. types.Select(a => (T)ActivatorUtilities.CreateInstance(serviceProvider, a!))];
    // }

    //public static MemberInfo? GetPropertyInfo(Expression propertyExpression)
    //{
    //    var memberExpr = propertyExpression as MemberExpression;
    //    if (memberExpr == null && propertyExpression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
    //    {
    //        memberExpr = unaryExpr.Operand as MemberExpression;
    //    }

    //    if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) { return memberExpr.Member; }
    //    return null;
    //}

    //public static string GetDescriptionProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    //{
    //    var propertyInfo = GetPropertyInfo(propertyExpression.Body);
    //    return propertyInfo == null
    //            ? null!
    //            : GetDescriptionProperty(propertyInfo.DeclaringType!, propertyInfo.Name);
    //}

    //    public static string GetDescriptionProperty<T>(Expression<Func<T, object>> propertyExpression) => GetDescriptionProperty<T, object>(propertyExpression);

    //public static string GetDisplayDescription(Enum enumValue)
    //{
    //    var enumValueAsString = enumValue.ToString();
    //    var memberInfo = enumValue.GetType().GetMember(enumValueAsString).FirstOrDefault();
    //    return memberInfo?.GetCustomAttribute<DisplayAttribute>()?.GetDescription() ?? enumValueAsString;
    //}

    public static string GetDisplayDescription<T>(string propertyName)
        => GetDisplayDescription(typeof(T), propertyName);

    public static string GetDisplayDescription(Type type, string propertyName)
    {
        var property = GetProperty(type, propertyName);
        if (property == null) { return propertyName.SplitCamelCase(); }

        var display = property.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(display?.GetName())) { return display.GetName()!; }

        return propertyName.SplitCamelCase();
    }

    public static string? GetDisplayFormatAttribute<T>(string propertyName)
        => GetDisplayFormatAttribute(typeof(T), propertyName);

    public static string? GetDisplayFormatAttribute(Type type, string propertyName)
        => GetProperty(type, propertyName)?
            .GetCustomAttribute<DisplayFormatAttribute>()?
            .DataFormatString;

    public static T? GetCustomAttribute<T>(Type type, string propertyName)
        where T : Attribute
        => GetProperty(type, propertyName)?.GetCustomAttribute<T>();

    private static PropertyInfo? GetProperty(Type type, string propertyName)
        => type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
}
