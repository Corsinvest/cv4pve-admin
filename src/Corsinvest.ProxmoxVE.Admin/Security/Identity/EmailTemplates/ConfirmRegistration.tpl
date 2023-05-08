@*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
*@
<!DOCTYPE html>
<html>

<head>
    <title>Confirm Registration @Model.ApplicationName</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
</head>

<body>
    <table border="0" width="100%" cellspacing="0" cellpadding="0">
        <tbody>
            <tr>
                <td>
                    <table border="0">
                        <tbody>
                            <tr>
                                <td>
                                    Hi there @Model.User.UserName 
                                    <br />
                                    Thanks for registering.
                                    <br />
                                    There's one more thing to do before you get started. Please confirm your email address by following the <a href="@Model.Url" target="_blank">link</a>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <table border="0">
                        <tbody>
                            <tr>
                                <td>
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td>Cheers, <br />The @Model.ApplicationName Crew</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>

</body>
</html>