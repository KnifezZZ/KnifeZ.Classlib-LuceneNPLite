<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebDemo.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="qKey" runat="server"></asp:TextBox>
            <asp:Button ID="btn_Search" runat="server" Text="查询" OnClick="btn_Search_Click"></asp:Button>
            <asp:Button ID="btn_AddIndex" runat="server" Text="添加索引" OnClick="btn_AddIndex_Click"></asp:Button>
            <asp:Button ID="btn_CreateAllIndex" runat="server" Text="添加索引" OnClick="btn_CreateAllIndex_Click"></asp:Button>

            <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>
