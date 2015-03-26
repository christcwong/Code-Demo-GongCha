<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportSalesData.aspx.cs" Inherits="GongChaWebApplication.Helpers.ImportSales.ImportSalesData" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="ImportSalesData.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
            <Services>
                <asp:ServiceReference Path="~/Helpers/ImportSales/ImportSalesDataWebService.asmx" />
            </Services>
        </asp:ScriptManager>

        <p>Gong Cha Import Sales data</p>
        Member: <asp:DropDownList ID="ddlMember" runat="server"></asp:DropDownList>
        <br />
        Store: <asp:DropDownList ID="ddlStore" runat="server"></asp:DropDownList>
        <br />
        <asp:FileUpload ID="fuSalesData" runat="server" />
        <asp:Button ID="btnImport" runat="server" Text="Import" OnClick="btnImport_Click" />
        <br />
        Status:
        <asp:Label ID="lblOutput" runat="server" Text=""></asp:Label>

        <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        <input id="btnCallDateTime" type="button" value="Call Web Service" onclick="CallDateTime()" />--%>
    </div>
    </form>
</body>
</html>
