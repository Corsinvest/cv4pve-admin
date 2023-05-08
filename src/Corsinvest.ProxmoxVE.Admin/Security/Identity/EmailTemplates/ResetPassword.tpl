@*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
*@
<!DOCTYPE html>
<html>

<head>
    <title>Reset Password @Model.ApplicationName</title>
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
                                    Forgot your password? No worries!
                                    <br />
                                    We'll help you set up a new one in just a few steps.
                                    <ul>
                                        <li>Visit this <a href="@Model.Url" target="_blank">link</a></li>
                                        <li>And setup a new password for your account</li>
                                    </ul>
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