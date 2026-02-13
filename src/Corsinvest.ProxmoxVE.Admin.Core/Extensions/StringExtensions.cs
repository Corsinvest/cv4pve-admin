/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class StringExtensions
{
    private static readonly string[] separator = ["\r\n", "\r", "\n"];
    public static string[] SplitNewLine(this string value) => value.Split(separator, StringSplitOptions.None);

    public static string EnsureEndsWith(this string str, char toEndWith) => EnsureEndsWith(str, toEndWith.ToString());

    public static string SplitCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) { return input; }

        var result = new StringBuilder();
        result.Append(input[0]);

        for (var i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
            {
                result.Append(' ');
            }

            result.Append(input[i]);
        }

        return result.ToString();
    }

    public static string EnsureEndsWith(this string str, string ending)
    {
        ArgumentNullException.ThrowIfNull(ending);
        if (str == null) { return ending; }
        var result = str;

        // Right() is 1-indexed, so include these cases
        // * Append no characters
        // * Append up to N characters, where N is ending length
        for (var i = 0; i <= ending.Length; i++)
        {
            var tmp = result + ending.Right(i);
            if (tmp.EndsWith(ending)) { return tmp; }
        }

        return result;
    }

    private static string Right(this string value, int length)
    {
        ArgumentNullException.ThrowIfNull(value);
        return length < 0
            ? throw new ArgumentOutOfRangeException(nameof(length), length, "Length is less than zero")
            : length < value.Length ? value[^length..] : value;
    }
}
