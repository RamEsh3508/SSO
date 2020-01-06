<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogIn.aspx.cs" Inherits="AzureADTest.LogIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
                <h1>Login to the Identity Provider</h1>
                <table border="0">
                    <tr>
                        <td>
                            <p>User name:</p>
                        </td>
                        <td>
                            <asp:TextBox ID="userNameTextBox" runat="server">testuser</asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <p>Password:</p>
                        </td>
                        <td>
                            <asp:TextBox ID="passwordTextBox" runat="server" TextMode="Password"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Button ID="loginButton" runat="server" Text="Login" />
                        </td>
                    </tr>
                </table>
                <p>
                    <asp:Label ID="errorMessageLabel" runat="server" ForeColor="Red"></asp:Label>
                </p>
        </div>
    </form>
</body>
</html>
