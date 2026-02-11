namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IJobSchedule : IEnabled
{
    string CronExpression { get; set; }
    string CronExpressionDescriptor { get; }
    bool CronExpressionIsValid { get; }
    DateTimeOffset? CronNextExecution { get; }
}
