using System.Linq.Expressions;
using Hangfire;
using BackgroundJobHelper = Hangfire.BackgroundJob;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Services;

internal class BackgroundJobService : IBackgroundJobService
{
    public bool Delete(string jobId) => BackgroundJobHelper.Delete(jobId);
    public bool Delete(string jobId, string fromState) => BackgroundJobHelper.Delete(jobId, fromState);

    public string Enqueue(Expression<Func<Task>> methodCall) => BackgroundJobHelper.Enqueue(methodCall);
    public string Enqueue<T>(Expression<Action<T>> methodCall) => BackgroundJobHelper.Enqueue(methodCall);
    public string Enqueue(Expression<Action> methodCall) => BackgroundJobHelper.Enqueue(methodCall);
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => BackgroundJobHelper.Enqueue(methodCall);
    public static bool Requeue(string jobId) => BackgroundJobHelper.Requeue(jobId);
    public static bool Requeue(string jobId, string fromState) => BackgroundJobHelper.Requeue(jobId, fromState);

    public string Schedule(Expression<Action> methodCall, TimeSpan delay) => BackgroundJobHelper.Schedule(methodCall, delay);
    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) => BackgroundJobHelper.Schedule(methodCall, delay);
    public string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt) => BackgroundJobHelper.Schedule(methodCall, enqueueAt);
    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt) => BackgroundJobHelper.Schedule(methodCall, enqueueAt);
    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) => BackgroundJobHelper.Schedule(methodCall, delay);
    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => BackgroundJobHelper.Schedule(methodCall, delay);
    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt) => BackgroundJobHelper.Schedule(methodCall, enqueueAt);
    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt) => BackgroundJobHelper.Schedule(methodCall, enqueueAt);

    //Recurring Job
    public void Schedule(string recurringJobId, Expression<Action> methodCall, string cronExpression, TimeZoneInfo timezone)
        => RecurringJob.AddOrUpdate(recurringJobId,
                                    methodCall,
                                    cronExpression,
                                    new RecurringJobOptions { TimeZone = timezone });

    public void Schedule(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo timezone)
        => RecurringJob.AddOrUpdate(recurringJobId,
                                    methodCall,
                                    cronExpression,
                                    new RecurringJobOptions { TimeZone = timezone });

    public void Schedule<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, TimeZoneInfo timezone)
        => RecurringJob.AddOrUpdate(recurringJobId,
                                    methodCall,
                                    cronExpression,
                                    new RecurringJobOptions { TimeZone = timezone });

    public void Schedule<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timezone)
        => RecurringJob.AddOrUpdate(recurringJobId,
                                    methodCall,
                                    cronExpression,
                                    new RecurringJobOptions
                                    {
                                        TimeZone = timezone ?? TimeZoneInfo.Local
                                    });

    public void RemoveIfExists(string recurringJobId) => RecurringJob.RemoveIfExists(recurringJobId);
    public void TriggerJob(string recurringJobId) => RecurringJob.TriggerJob(recurringJobId);

    public string MakeJobId<T>(string clusterName, params object?[] args)
        => $"{typeof(T).FullName}-{clusterName}" +
                (args?.Length > 0
                 ? "-" + args!.JoinAsString("-")
                 : string.Empty);

    public void ScheduleOrRemove<T>(Expression<Func<T, Task>> methodCall,
                                    string cronExpression,
                                    bool enabled,
                                    string clusterName,
                                    params object?[] args)
    {
        var jobId = MakeJobId<T>(clusterName, args);
        if (enabled)
        {
            Schedule(jobId, methodCall, cronExpression, TimeZoneInfo.Local);
        }
        else
        {
            RemoveIfExists(jobId);
        }
    }

    public void RemoveIfExists<T>(string clusterName, params object?[] args) => RemoveIfExists(MakeJobId<T>(clusterName, args));

    public int DeleteFails()
    {
        var monitor = JobStorage.Current.GetMonitoringApi();
        var failedJobs = monitor.FailedJobs(0, int.MaxValue);

        var count = 0;
        foreach (var failedJob in failedJobs)
        {
            if (Delete(failedJob.Key))
            {
                count++;
            }
        }

        return count;
    }
}
