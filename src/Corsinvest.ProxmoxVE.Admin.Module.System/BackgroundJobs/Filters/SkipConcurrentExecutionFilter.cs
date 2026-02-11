using System.Globalization;
using Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Extensions;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Filters;

public class SkipConcurrentExecutionFilter(ILogger<SkipConcurrentExecutionFilter> Logger,
                                           TimeSpan lockTimeout,
                                           TimeSpan fingerprintTimeout) : JobFilterAttribute, IClientFilter, IServerFilter
{
    private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan FingerprintTimeout = TimeSpan.FromHours(1);

    private readonly ILogger<SkipConcurrentExecutionFilter> _logger = Logger ?? throw new ArgumentNullException(nameof(Logger));

    public SkipConcurrentExecutionFilter(ILogger<SkipConcurrentExecutionFilter> logger)
        : this(logger, DefaultLockTimeout, FingerprintTimeout) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void OnCreated(CreatedContext context) { }

    public void OnCreating(CreatingContext filterContext)
    {
        //if (!filterContext.Job.SkipConcurrentExecution()) { return; }

        //_logger.LogInformation("Creating job '{JobName}'...", filterContext.Job.Method.Name);

        if (!AddFingerprintIfNotExists(filterContext.Connection, filterContext.Job))
        {
            _logger.LogWarning("Job '{JobName}' is already running, skipping...", filterContext.Job.Method.Name);
            filterContext.Canceled = true;
        }
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        //if (!filterContext.BackgroundJob.Job.SkipConcurrentExecution()) { return; }
        _logger.LogInformation("Performing job '{JobName}'...", filterContext.BackgroundJob.Job.Method.Name);
        RemoveFingerprint(filterContext.Connection, filterContext.BackgroundJob.Job);
    }

    public void OnPerforming(PerformingContext context) { }

    private bool AddFingerprintIfNotExists(IStorageConnection connection, Job job)
    {
        _logger.LogInformation("Adding fingerprint for job '{JobName}'...", job.Method.Name);

        using (connection.AcquireDistributedLock(job.GetFingerprintLockKey(), lockTimeout))
        {
            var fingerprint = connection.GetAllEntriesFromHash(job.GetFingerprintKey());

            if (fingerprint is not null
                && fingerprint.TryGetValue("Timestamp", out var value)
                && DateTimeOffset.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var timestamp)
                && DateTimeOffset.UtcNow <= timestamp.Add(fingerprintTimeout))
            {
                _logger.LogInformation("Fingerprint for job '{JobName}' already exists, skipping...", job.Method.Name);
                return false;
            }

            _logger.LogInformation("Fingerprint for job '{JobName}' does not exist, adding...", job.Method.Name);

            connection.SetRangeInHash(job.GetFingerprintKey(), new Dictionary<string, string>
            {
                { "Timestamp", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture) }
            });

            return true;
        }
    }

    private void RemoveFingerprint(IStorageConnection connection, Job job)
    {
        _logger.LogInformation("Removing fingerprint for job '{JobName}'...", job.Method.Name);

        using (connection.AcquireDistributedLock(job.GetFingerprintLockKey(), lockTimeout))
        using (var transaction = connection.CreateWriteTransaction())
        {
            transaction.RemoveHash(job.GetFingerprintKey());
            transaction.Commit();

            _logger.LogInformation("Fingerprint for job '{JobName}' removed...", job.Method.Name);
        }
    }
}
