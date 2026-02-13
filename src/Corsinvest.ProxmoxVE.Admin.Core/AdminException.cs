/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core;

public class AdminException : Exception
{
    public AdminException() { }
    public AdminException(string message) : base(message) { }
    public AdminException(string message, Exception innerException) : base(message, innerException) { }
}
