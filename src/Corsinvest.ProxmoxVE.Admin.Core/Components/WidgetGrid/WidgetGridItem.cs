namespace Corsinvest.ProxmoxVE.Admin.Core.Components.WidgetGrid;

public class WidgetGridItem
{
    public int Id { get; set; }
    public int Col { get; set; }
    public int Row { get; set; }
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string? TitleCss { get; set; }
    public string? BodyCss { get; set; }
    public string? Icon { get; set; }
    public RenderFragment? Template { get; set; }
}
