/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Xml.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Subscription;

public class Info
{
    [XmlElement("status")]
    public Status Status { get; set; }

    [XmlElement("validdirectory")]
    public string ServerId { get; set; } = default!;

    [XmlElement("productname")]
    public string ProductName { get; set; } = default!;

    [XmlIgnore]
    public DateTime? RegistrationDate { get; set; } = default!;

    [XmlElement("regdate")]
    public string RegistrationDateString
    {
        get => RegistrationDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
        set => RegistrationDate = DateTime.TryParse(value, out var date) ? date : null;
    }

    [XmlIgnore]
    public DateTime? NextDueDate { get; set; } = default!;

    [XmlElement("nextduedate")]
    public string NextDueDatetring
    {
        get => NextDueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;
        set => NextDueDate = DateTime.TryParse(value, out var date) ? date : null;
    }

    [XmlElement("message")]
    public string Message { get; set; } = default!;
}