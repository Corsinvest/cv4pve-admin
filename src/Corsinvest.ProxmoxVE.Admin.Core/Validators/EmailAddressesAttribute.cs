/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class EmailAddressesAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string emails || string.IsNullOrWhiteSpace(emails)) { return true; }

        var validator = new EmailAddressAttribute();
        return emails.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                     .All(a => validator.IsValid(a.Trim()));
    }
}
