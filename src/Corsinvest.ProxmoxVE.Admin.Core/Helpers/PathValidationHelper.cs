namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

/// <summary>
/// Provides path validation utilities to prevent path traversal attacks and invalid path characters.
/// </summary>
public static class PathValidationHelper
{
    /// <summary>
    /// Validates a path component to prevent path traversal attacks and invalid characters.
    /// </summary>
    /// <param name="component">The path component to validate (e.g., cluster name, file name, directory name)</param>
    /// <param name="paramName">The parameter name for exception messages</param>
    /// <exception cref="ArgumentException">Thrown when component is invalid</exception>
    public static void ValidatePathComponent(string component, string paramName)
    {
        if (string.IsNullOrWhiteSpace(component))
        {
            throw new ArgumentException("Path component cannot be null or empty.", paramName);
        }

        // Check for path traversal patterns
        if (component.Contains("..") || component.Contains('/') || component.Contains('\\'))
        {
            throw new ArgumentException($"Invalid path component: '{component}'. Path traversal patterns are not allowed.", paramName);
        }

        // Check for invalid path characters
        if (component.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"Invalid path component: '{component}'. Contains invalid characters.", paramName);
        }
    }

    /// <summary>
    /// Validates a cluster name for use in file system paths.
    /// </summary>
    /// <param name="clusterName">The cluster name to validate</param>
    /// <exception cref="ArgumentException">Thrown when cluster name is invalid</exception>
    public static void ValidateClusterName(string clusterName)
        => ValidatePathComponent(clusterName, nameof(clusterName));
}
