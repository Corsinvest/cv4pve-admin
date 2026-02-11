using System.ComponentModel.DataAnnotations;
using Corsinvest.ProxmoxVE.Admin.Core.Configuration;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect;

public class Settings : JobScheduleBase, IModuleSettings
{
    public Settings() => CronExpression = "0 6 * * *";

    [Required] public string ClusterName { get; set; } = default!;

    [Required]
    [Display(Name = "Directory or file")]
    public string PathsToBackup { get; set; } = @"
/etc/.
/var/lib/pve-cluster/.
/var/lib/ceph/.
/root/.ssh
/root/scripts
/root/backup
/root/*.sh
/root/*.conf
/root/.bashrc
/root/.profile";

    public class FolderSettings : IEnabled
    {
        public bool Enabled { get; set; }

        public int Keep { get; set; } = 30;
    }

    public FolderSettings Folder { get; set; } = new();

    //    public class GitSettings : IEnabled
    //    {
    //        public bool Enabled { get; set; }
    //        [Required] public string RemoteBranchName { get; set; } = string.Empty;
    //        [Required] public string RemoteUrl { get; set; } = string.Empty;
    //        public Credential Credential { get; set; } = new();
    //        [Required] public string DisplayName { get; set; } = default!;
    //        [Required, EmailAddress] public string Email { get; set; } = default!;

    //        public string GitIgnore { get; set; } = """
    //*/etc/pve/nodes/*/lrm_status
    //*/etc/pve/.rrd
    //""";
    //    }

    //    public GitSettings Git { get; set; } = new();

    public ExtendedData ExtendedData { get; set; } = [];
}
