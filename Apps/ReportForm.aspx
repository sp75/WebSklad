<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportForm.aspx.cs" Inherits="WebApplication1.Apps.ReportForm" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="На дату" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxDateEdit ID="OnDateDBEdit" runat="server" Theme="Moderno" Font-Size="Large" Width="360px">
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxDateEdit>
    <br />
    <dx:ASPxLabel ID="ASPxLabel2" runat="server" Text="Склад:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxComboBox ID="WhComboBox" runat="server" Width="360px" Theme="Moderno" ValueField="WId" EnableTheming="True" Font-Size="Large">
        <Columns>
            <dx:ListBoxColumn Caption="Назва" FieldName="Name" />
        </Columns>
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxComboBox>
    <br />

    <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text="Група:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxComboBox ID="GrpComboBox" runat="server" Height="20px" Width="360px" Theme="Moderno" ValueField="GrpId" ValueType="System.Int32" Font-Size="Large">
        <Columns>
            <dx:ListBoxColumn Caption="Назва" FieldName="Name" />
        </Columns>
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxComboBox>

    <br />
    <br />
    <dx:ASPxButton ID="OkButton" runat="server" Text="Сформувати" OnClick="OkButton_Click" Theme="Moderno" Width="360px" Font-Bold="False" Font-Size="Large" Height="50px">
        <FocusRectBorder BorderStyle="None" />
    </dx:ASPxButton>

    <dx:ASPxLabel ID="messageLabel" runat="server" Text="ASPxLabel" Visible="False">
    </dx:ASPxLabel>

</asp:Content>
