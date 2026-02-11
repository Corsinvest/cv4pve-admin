using System.Linq.Expressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class LegendCounter<TItem>
{
    [Parameter] public IEnumerable<TItem> Data { get; set; } = [];
    [Parameter] public bool ShowUnit { get; set; }
    [Parameter] public IEnumerable<LegendCounterItem> Counters { get; set; } = [];

    private enum TypeValue
    {
        Latest,
        Average,
        Min,
        Max
    }

    private string GetValue(LegendCounterItem item, TypeValue type)
    {
        var compiledSelector = item.Selector.Compile();

        var value = type switch
        {
            TypeValue.Latest => Data.Select(compiledSelector).FirstOrDefault(),
            TypeValue.Average => Data.Select(compiledSelector).DefaultIfEmpty(0).Average(),
            TypeValue.Min => Data.Select(compiledSelector).DefaultIfEmpty(0).Min(),
            TypeValue.Max => Data.Select(compiledSelector).DefaultIfEmpty(0).Max(),
            _ => 0
        };

        return item.Formatter != null
                    ? item.Formatter(value)
                    : !string.IsNullOrEmpty(item.StringFormat)
                        ? value.ToString(item.StringFormat)
                        : value.ToString();
    }

    public record LegendCounterItem(string Name,
                                    string Unit,
                                    string StringFormat,
                                    Expression<Func<TItem, double>> Selector)
    {
        public Func<double, string>? Formatter { get; set; }
    }
}
