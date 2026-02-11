using Hangfire.Common;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.BackgroundJobs.Extensions;

internal static class JobExtensions
{
    //public static bool SkipConcurrentExecution(this Job job)
    //    => job.Method.GetCustomAttributes(typeof(SkipConcurrentExecutionAttribute), false).Length > 0;

    public static string GetFingerprintLockKey(this Job job) => $"{job.GetFingerprintKey()}:lock";
    public static string GetFingerprintKey(this Job job) => $"fingerprint:{job.GetFingerprint()}";

    private static string GetFingerprint(this Job job)
    {
        if (job.Type == null || job.Method == null) { return string.Empty; }
        var parameters = string.Empty;

        if (job.Args is not null) { parameters = string.Join(".", job.Args); }

        return $"{job.Type.FullName}.{job.Method.Name}.{parameters}";
    }
}
