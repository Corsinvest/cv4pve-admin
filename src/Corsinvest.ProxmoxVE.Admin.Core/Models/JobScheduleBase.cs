using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Admin.Core.Validators;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class JobScheduleBase : IJobSchedule
{
    public bool Enabled { get; set; }

    [Required]
    [DisplayName("Cron Schedule")]
    [CronExpressionValidator]
    public string CronExpression { get; set; } = default!;

    [JsonIgnore]
    [NotMapped]
    [DisplayName("Next Execution")]
    public DateTimeOffset? CronNextExecution => Enabled ? CronHelper.NextOccurrence(CronExpression) : null;

    [JsonIgnore]
    [NotMapped]
    [DisplayName("Cron Descriptor")]
    public virtual string CronExpressionDescriptor => CronHelper.GetDescription(CronExpression);

    [JsonIgnore]
    [NotMapped]
    public bool CronExpressionIsValid => CronHelper.IsValid(CronExpression);
}
