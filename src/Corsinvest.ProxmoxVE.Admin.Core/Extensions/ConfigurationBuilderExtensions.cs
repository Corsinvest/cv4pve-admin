/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a JSON configuration file, skipping it with a warning if the file contains invalid JSON.
    /// Returns a warning message if the file was skipped, null otherwise.
    /// </summary>
    public static (IConfigurationBuilder Builder, string? Warning) AddJsonFileSafe(this IConfigurationBuilder builder, string path, bool reloadOnChange = true)
    {
        if (File.Exists(path))
        {
            try { JsonDocument.Parse(File.ReadAllText(path)); }
            catch (JsonException ex)
            {
                var warning = $"'{path}' contains invalid JSON and will be ignored. Fix the file and restart. Error: {ex.Message}";
                Console.Error.WriteLine($"WARNING: {warning}");
                return (builder, warning);
            }
        }
        else
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, "{}");
                Console.WriteLine($"INFO: Created '{path}' with empty configuration.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WARNING: Could not create '{path}': {ex.Message}");
            }
        }

        return (builder.AddJsonFile(path, optional: true, reloadOnChange: reloadOnChange), null);
    }
}
