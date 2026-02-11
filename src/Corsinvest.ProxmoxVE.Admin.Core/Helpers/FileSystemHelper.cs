namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class FileSystemHelper
{
    public static void RemoveReadOnlyAttribute(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes &= ~FileAttributes.ReadOnly;
        }
    }
}
