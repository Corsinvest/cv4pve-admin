using Corsinvest.ProxmoxVE.Admin.Core.Components.Parameters;
using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;
using Corsinvest.ProxmoxVE.Admin.Core.Search;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Layout;

public partial class CommandPalette(IEnumerable<ISearchProvider> searchProviders,
                                    NavigationManager navigationManager,
                                    DialogService dialogService,
                                    IServiceProvider serviceProvider) : IClusterName
{
    [Parameter] public string ClusterName { get; set; } = default!;

    private bool IsOpen { get; set; }
    private bool IsSearch { get; set; }
    private RadzenTextBox? TextBoxRef { get; set; }
    private RadzenListBox<SearchResultItem>? ListBoxRef { get; set; }
    private SearchResultItem? SelectedItem { get; set; }

    private string _searchText = string.Empty;
    private string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            SelectedItem = null;
        }
    }

    private IEnumerable<ISearchProvider> ValidProviders
        => searchProviders.Where(a => !a.RequireClusterName || !string.IsNullOrEmpty(ClusterName));

    private async Task OnSearchInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        FilteredItems = await GetFilteredItemsAsync();
        StateHasChanged();
    }

    private IEnumerable<SearchResultItem> FilteredItems { get; set; } = [];

    private async Task<IEnumerable<SearchResultItem>> GetFilteredItemsAsync()
    {
        IsSearch = true;
        var query = SearchText?.Trim() ?? string.Empty;
        var context = BuildSearchContext(query);

        // Aggregate results from all providers
        var results = new List<SearchResultItem>();
        foreach (var provider in ValidProviders)
        {
            results.AddRange(await provider.SearchAsync(context));
        }

        IsSearch = false;

        // If empty query, show suggestions (modules + commands)
        return string.IsNullOrEmpty(query)
                ? results.Where(x => x.ResultType is SearchResultType.Module or SearchResultType.Command) //.Take(10)
                : results.Take(15);
    }

    private SearchContext BuildSearchContext(string query)
    {
        var filters = ParseFilters(query, out var searchText);
        return new SearchContext(query, searchText, filters, ClusterName);
    }

    private Dictionary<SearchFilter, string> ParseFilters(string query, out string remainingText)
    {
        var filters = new Dictionary<SearchFilter, string>();
        var textParts = new List<string>();
        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Get all filter prefixes from providers
        var allFilters = ValidProviders.SelectMany(p => p.Filters).ToList();

        foreach (var token in tokens)
        {
            var matched = false;

            // Check for > prefix (commands) - just add to search text
            if (token.StartsWith('>'))
            {
                if (token.Length > 1) { textParts.Add(token[1..]); }
                continue;
            }

            // Check for filter prefixes (vm:, ct:, node:, etc.)
            if (token.Contains(':'))
            {
                var parts = token.Split(':', 2);
                var prefix = parts[0].ToLower() + ":";
                var value = parts.Length > 1 ? parts[1] : string.Empty;

                var filter = allFilters.FirstOrDefault(a => a.Prefix.Equals(prefix, StringComparison.OrdinalIgnoreCase));

                if (filter != null)
                {
                    filters[filter] = value;
                    matched = true;

                    // Add filter value to search text for filtering (e.g., vm:101 -> search for "101")
                    if (!string.IsNullOrEmpty(value)) { textParts.Add(value); }
                }
            }

            if (!matched) { textParts.Add(token); }
        }

        remainingText = string.Join(" ", textParts);
        return filters;
    }

    private record FooterHint(string Text, string Label, BadgeStyle BadgeStyle);

    private IEnumerable<FooterHint> GetFooterHints()
    {
        // Navigation hints (always shown)
        yield return new FooterHint("↑↓", L["navigate"], BadgeStyle.Base);
        yield return new FooterHint(L["Enter"], L["select"], BadgeStyle.Base);
        yield return new FooterHint(L["Esc"], L["close"], BadgeStyle.Base);

        // Filter hints from providers
        foreach (var provider in ValidProviders)
        {
            foreach (var filter in provider.Filters.Take(4)) // Limit to avoid overflow
            {
                yield return new FooterHint(filter.Prefix, filter.Label, filter.BadgeStyle);
            }
        }

        // Command hint if any provider has commands
        if (ValidProviders.Any(p => p.Commands.Any()))
        {
            yield return new FooterHint(">", L["Cmd"], BadgeStyle.Secondary);
        }
    }

    private string Placeholder
        => L["Search ({0} > to filter)", ValidProviders.SelectMany(p => p.Filters)
                                                            .Take(3)
                                                            .Select(f => f.Prefix.TrimEnd(':'))
                                                            .ToList()
                                                            .JoinAsString("")];

    public async Task Open()
    {
        IsOpen = true;
        SearchText = string.Empty;
        SelectedItem = null;
        FilteredItems = await GetFilteredItemsAsync();
        StateHasChanged();

        await Task.Delay(100);
        if (TextBoxRef != null) { await TextBoxRef.FocusAsync(); }
    }

    public void Close()
    {
        IsOpen = false;
        FilteredItems = [];
        SelectedItem = null;
        StateHasChanged();
    }

    //private void ClearSearch()
    //{
    //    SearchText = string.Empty;
    //    SelectedItem = null;
    //}

    private async Task SelectItem(SearchResultItem item)
    {
        // Handle based on result type
        if (item.ResultType == SearchResultType.Command && item.Command != null)
        {
            await HandleCommandSelection(item.Command);
        }
        else if (item.Execute != null)
        {
            Close();
            await item.Execute(new SearchExecutionContext(item, ClusterName, navigationManager));
        }
        else if (!string.IsNullOrEmpty(item.Url))
        {
            navigationManager.NavigateTo(item.Url);
            Close();
        }
    }

    private async Task HandleCommandSelection(SearchCommand command)
    {
        Close();

        if (command.RequiresDialog)
        {
            await OpenCommandDialog(command);
        }
        else
        {
            await ExecuteCommand(command, []);
        }
    }

    private async Task OpenCommandDialog(SearchCommand command)
    {
        var values = new Dictionary<string, object?>();
        var clusterName = ClusterName;
        Func<ParameterMetadata, Task<DataSourceContext>> getDataSourceContext
            = _ => Task.FromResult(new DataSourceContext(values) { ClusterName = clusterName });

        var result = await dialogService.OpenAsync<ParameterDialog>(command.Label,
                                                                    new Dictionary<string, object>
                                                                    {
                                                                        { nameof(ParameterDialog.Icon), command.Icon },
                                                                        { nameof(ParameterDialog.Title), command.Label },
                                                                        { nameof(ParameterDialog.Description), command.Description },
                                                                        { nameof(ParameterDialog.Parameters), command.Parameters },
                                                                        { nameof(ParameterDialog.Values), values },
                                                                        { nameof(ParameterDialog.GetDataSourceContext), getDataSourceContext },
                                                                    },
                                                                    new DialogOptions
                                                                    {
                                                                        CloseDialogOnOverlayClick = true,
                                                                        Width = "600px",
                                                                        Top = "50px",
                                                                        Left = "50%",
                                                                        Style = "transform: translateX(-50%);",
                                                                        ShowClose = true,
                                                                    });

        if (result is Dictionary<string, object?> parameters) { await ExecuteCommand(command, parameters); }
    }

    private async Task ExecuteCommand(SearchCommand command, Dictionary<string, object?> parameters)
    {
        if (command.Execute != null)
        {
            await command.Execute.Invoke(new CommandExecutionContext(command, parameters, ClusterName, serviceProvider));
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Escape": Close(); break;

            case "ArrowDown":
                {
                    var items = FilteredItems.ToList();
                    if (items.Count > 0)
                    {
                        var currentIndex = SelectedItem != null ? items.IndexOf(SelectedItem) : -1;
                        SelectedItem = items[Math.Min(currentIndex + 1, items.Count - 1)];
                    }
                }
                break;

            case "ArrowUp":
                {
                    var items = FilteredItems.ToList();
                    if (items.Count > 0)
                    {
                        var currentIndex = SelectedItem != null ? items.IndexOf(SelectedItem) : items.Count;
                        if (currentIndex <= 0)
                        {
                            // Already at first item, focus textbox
                            SelectedItem = null;
                            if (TextBoxRef != null) { await TextBoxRef.FocusAsync(); }
                        }
                        else
                        {
                            SelectedItem = items[currentIndex - 1];
                        }
                    }
                }
                break;

            case "Enter":
                if (SelectedItem != null) { await SelectItem(SelectedItem); }
                break;
        }
    }

    private async Task HandleListKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Escape": Close(); break;

            case "ArrowUp":
                // Check if previous selection was the first item
                //var items = FilteredItems.ToList();
                //var prevIndex = SelectedItem != null ? items.IndexOf(SelectedItem) : -1;
                //if (prevIndex == 0 && TextBoxRef != null)
                //{
                //    await TextBoxRef.FocusAsync();
                //}
                break;

            case "Enter":
                if (SelectedItem != null) { await SelectItem(SelectedItem); }
                break;
        }
    }

    private async Task OnListBoxChange(object value)
    {
        // Double-click or Enter on list item
        if (SelectedItem != null) { await SelectItem(SelectedItem); }
    }
}
