<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="rep6.aspx.cs" Inherits="WebSklad.Reports.rep6" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <asp:Label ID="Label1" runat="server" Text="Label" Font-Size="Large"></asp:Label>
     <br />
    <br />
    <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="На дату" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxDateEdit ID="OnDateDBEdit" runat="server" Theme="Moderno" Font-Size="Large" Width="360px">
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxDateEdit>
    <br />
    <br />
    <dx:ASPxButton ID="OkButton" runat="server" Text="Сформувати" OnClick="OkButton_Click" Theme="Moderno" Width="360px" Font-Bold="False" Font-Size="Large" Height="50px">
        <FocusRectBorder BorderStyle="None" />
    </dx:ASPxButton>

    <dx:ASPxLabel ID="messageLabel" runat="server" Text="ASPxLabel" Visible="False">
    </dx:ASPxLabel>

</asp:Content>
