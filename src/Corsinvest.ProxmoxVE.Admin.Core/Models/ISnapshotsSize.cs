namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface ISnapshotsSize
{
    [Display(Name = "Snapshots Size")]
    [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
    double SnapshotsSize { get; set; }
}
