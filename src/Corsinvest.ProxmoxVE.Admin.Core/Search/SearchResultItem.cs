namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record SearchTag(string Value, BadgeStyle Style = BadgeStyle.Primary);

public class SearchResultItem
{
    public string Title { get; set; } = default!;
    public string Subtitle { get; set; } = default!;
    public IEnumerable<string>? ExtraInfo { get; set; }
    public string Icon { get; set; } = default!;
    public string Color { get; set; } = default!;
    public SearchResultType ResultType { get; set; }
    public string Category { get; set; } = default!;
    public string? Url { get; set; }
    public IEnumerable<SearchTag>? Tags { get; set; }
    public SearchCommand? Command { get; set; }
    public Func<SearchExecutionContext, Task>? Execute { get; set; }

    public const string CommandColor = "var(--rz-base-600)";
    public const string ModuleColor = "var(--rz-primary)";

    public bool MatchesSearch(string searchText) =>
        string.IsNullOrWhiteSpace(searchText)
        || (Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
        || (Subtitle?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
        || (ExtraInfo?.Any(x => x.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ?? false);

    public static IEnumerable<SearchResultItem> CommandsToResult(ISearchProvider searchProvider)
                    => searchProvider.Commands.Select(c => new SearchResultItem
                    {
                        Title = c.Prefix,
                        Subtitle = c.Description,
                        Icon = c.Icon,
                        Color = CommandColor,
                        ResultType = SearchResultType.Command,
                        Category = nameof(SearchResultType.Command),
                        Command = c
                    });
}
