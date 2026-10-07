/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Diagnostic.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Models;

public class IgnoredIssue : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string ClusterName { get; set; } = default!;
    public string? IdResource { get; set; }
    public string? ErrorCode { get; set; }
    public DiagnosticResultGravity Gravity { get; set; }
    public DiagnosticResultContext Context { get; set; }
    public string? SubContext { get; set; }
    public string? Description { get; set; }

    // The engine reads the text fields as regular expressions, found anywhere in the value.
    // An empty field matches any value. Id, error code and sub context are anchored, to match
    // one value only; the description is not, because many descriptions carry values that
    // change at every scan
    public DiagnosticIgnoreRule ToRule()
        => new()
        {
            Id = Exact(IdResource),
            ErrorCode = Exact(ErrorCode),
            SubContext = Exact(SubContext),
            Description = string.IsNullOrEmpty(Description) ? null : Regex.Escape(Description),
            Context = Context,
            Gravity = Gravity
        };

    private static string? Exact(string? value)
        => string.IsNullOrEmpty(value) ? null : $"^{Regex.Escape(value)}$";
}
