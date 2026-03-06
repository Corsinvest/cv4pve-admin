using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Service;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskTracker(this IServiceCollection services)
    {
        services.AddSingleton<ITaskTrackerService, TaskTrackerService>();
        services.AddTransient<Job>();
        return services;
    }

    public static async Task AbandonStaleTasksAsync(this IServiceScope scope)
        => await scope.GetRequiredService<ITaskTrackerService>().AbandonStaleTasksAsync();

}
