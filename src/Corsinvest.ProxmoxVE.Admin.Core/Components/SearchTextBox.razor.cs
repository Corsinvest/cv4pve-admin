/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class SearchTextBox<TItem> : IDisposable
{
    [Parameter] public string Style { get; set; } = "width:220px;";

    /// <summary>
    /// Explicit properties to search. If empty, all compatible properties of TItem are used.
    /// Usage: Properties="@([x => x.Description, x => x.Node])"
    /// </summary>
    [Parameter] public IEnumerable<Expression<Func<TItem, object?>>> Properties { get; set; } = [];

    /// <summary>
    /// Logical operator between columns (default Or — any column matches).
    /// </summary>
    [Parameter] public LogicalFilterOperator LogicalFilterOperator { get; set; } = LogicalFilterOperator.Or;

    /// <summary>
    /// Case sensitivity for string filters (default CaseInsensitive).
    /// </summary>
    [Parameter] public FilterCaseSensitivity FilterCaseSensitivity { get; set; } = FilterCaseSensitivity.CaseInsensitive;

    /// <summary>
    /// Raised when the search text changes, with the generated FilterDescriptors.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<FilterDescriptor>> FiltersChanged { get; set; }

    /// <summary>
    /// Debounce delay in milliseconds (default 300).
    /// </summary>
    [Parameter] public int DebounceMs { get; set; } = 300;

    private string Text { get; set; } = string.Empty;

    private CancellationTokenSource? _cts;

    private async Task OnValueChanged(string value)
    {
        Text = value;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        if (string.IsNullOrWhiteSpace(Text))
        {
            await FiltersChanged.InvokeAsync([]);
            return;
        }

        var captured = Text;
        try
        {
            await Task.Delay(DebounceMs, token);
            await FiltersChanged.InvokeAsync(BuildFilters(captured));
        }
        catch (TaskCanceledException) { }
    }

    public async Task ClearAsync()
    {
        _cts?.Cancel();
        Text = string.Empty;
        await InvokeAsync(StateHasChanged);
        await FiltersChanged.InvokeAsync([]);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private IEnumerable<FilterDescriptor> BuildFilters(string text)
    {
        foreach (var (name, type) in GetSearchableProperties())
        {
            if (type == typeof(string))
            {
                yield return new FilterDescriptor
                {
                    Property = name,
                    FilterValue = text,
                    FilterOperator = FilterOperator.Contains,
                    LogicalFilterOperator = LogicalFilterOperator
                };
            }
            else if (IsNumeric(type) && TryParseNumeric(text, type, out var numericValue))
            {
                yield return new FilterDescriptor
                {
                    Property = name,
                    FilterValue = numericValue,
                    FilterOperator = FilterOperator.Equals,
                    LogicalFilterOperator = LogicalFilterOperator,
                    Type = type
                };
            }
            else if (type.IsEnum && TryParseEnum(text, type, out var enumValue))
            {
                yield return new FilterDescriptor
                {
                    Property = name,
                    FilterValue = enumValue,
                    FilterOperator = FilterOperator.Equals,
                    LogicalFilterOperator = LogicalFilterOperator,
                    Type = type
                };
            }
        }
    }

    private IEnumerable<(string Name, Type Type)> GetSearchableProperties()
    {
        var explicitProps = Properties.Select(GetPropertyName)
                                      .Where(n => n != null)
                                      .ToHashSet();

        var hasFilter = explicitProps.Count > 0;

        return typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead && (!hasFilter || explicitProps.Contains(p.Name)))
                            .Select(p => (p.Name, Type: Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType))
                            .Where(x => x.Type == typeof(string) || IsNumeric(x.Type) || x.Type.IsEnum);
    }

    private static string? GetPropertyName(Expression<Func<TItem, object?>> expression)
        => expression.Body switch
        {
            MemberExpression m => m.Member.Name,
            UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
            _ => null
        };

    private static bool IsNumeric(Type type)
        => type.GetInterfaces().Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(INumber<>));

    private static bool TryParseNumeric(string text, Type type, out object? value)
    {
        value = null;
        try { value = Convert.ChangeType(text, type); return true; }
        catch { return false; }
    }

    private static bool TryParseEnum(string text, Type type, out object? value)
    {
        value = null;
        if (Enum.TryParse(type, text, ignoreCase: true, out var result))
        {
            value = result;
            return true;
        }
        return false;
    }
}
