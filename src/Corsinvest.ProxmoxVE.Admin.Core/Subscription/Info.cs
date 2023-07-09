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
    public DateTime RegistrationDate { get; set; } = default!;

    [XmlElement("regdate")]
    public string RegistrationDateString
    {
        get { return RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss"); }
        set { RegistrationDate = DateTime.Parse(value); }
    }

    [XmlElement("nextduedate")]
    public DateTime? NextDueDate { get; set; } = default!;

    [XmlElement("message")]
    public string Message { get; set; } = default!;
}

/*
<status>Suspended</status>
<registeredname>Frank Lupo</registeredname>
<companyname></companyname>
<email>franklupog@gmail.com</email>
<serviceid>5</serviceid>
<productid>1</productid>
<productname>cv4pve-admin HOME</productname>
<regdate>2023-06-25 00:00:00</regdate>
<nextduedate>2023-06-27</nextduedate>
<billingcycle>Annually</billingcycle>
<validdomain>corsinvest.it,www.corsinvest.it</validdomain>
<validip>localhost</validip>
<validdirectory>A0A12C8D4C93E75AD7737C2294969A0F</validdirectory>
<configoptions></configoptions>
<customfields>Notes=|Internal Order Number=</customfields>
<addons></addons>
<md5hash></md5hash>
 */