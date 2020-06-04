<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="THOMAS_MailTest2.aspx.vb" Inherits="OFFICE.THOMAS_MailTest2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:RadioButtonList ID="RadioButtonList2" runat="server">
                <asp:ListItem Text="デモ環境" Value="demo" Selected="True"></asp:ListItem>
                <asp:ListItem Text="テスト環境" Value="test"></asp:ListItem>

            </asp:RadioButtonList>
            <br />
<%--            <asp:RadioButton ID="RadioButton1" runat="server" Checked="True" GroupName="brevent" Text="Input Request(POL)" />
            <asp:RadioButton ID="RadioButton2" runat="server" GroupName="brevent" Text="Input Request(POD)" />
            <asp:RadioButton ID="RadioButton3" runat="server" GroupName="brevent" Text="Entry Cost(POL)" />
            <asp:RadioButton ID="RadioButton4" runat="server" GroupName="brevent" Text="Entry Cost(POD)" />
            <asp:RadioButton ID="RadioButton5" runat="server" GroupName="brevent" Text="Apply" />--%>
            <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                <asp:ListItem Text="Input Request(POL)" Value="BRS_CostIn_POL" Selected="True"></asp:ListItem>
                <asp:ListItem Text="Input Request(POD)" Value="BRS_CostIn_POD"></asp:ListItem>
                <asp:ListItem Text="Entry Cost(POL)" Value="BRS_CostFn_POL"></asp:ListItem>
                <asp:ListItem Text="Entry Cost(POD)" Value="BRS_CostFn_POD"></asp:ListItem>
                <asp:ListItem Text="Apply" Value="BRS_Apply"></asp:ListItem>
            </asp:RadioButtonList>
            <br />
            <asp:Label ID="Label1" runat="server" Text="Breaker ID："></asp:Label>
            <asp:TextBox ID="TextBox3" runat="server">BT1807_0121_01</asp:TextBox>
            <br />
            <asp:Button ID="Button1" runat="server" Text="メール送信" />

            <asp:Label ID="Label2" runat="server"></asp:Label>

            <br />
            <br />
            <br />
            <br />
        </div>
    </form>
</body>
</html>
