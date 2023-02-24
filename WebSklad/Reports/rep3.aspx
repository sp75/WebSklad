<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="rep3.aspx.cs" Inherits="WebSklad.Reports.rep3" %>
<%@ Register assembly="DevExpress.Web.v21.1, Version=21.1.3.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <br />
    <asp:Label ID="Label1" runat="server" Text="Label" Font-Size="Large"></asp:Label>
     <br />
    <br />
    <dx:ASPxLabel ID="ASPxLabel1" runat="server" Text="Період з:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxDateEdit ID="StartDateEdit" runat="server" Theme="Moderno" Font-Size="Large" EditFormat="DateTime" Width="360px">
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxDateEdit>
        <br />
     <dx:ASPxLabel ID="ASPxLabel4" runat="server" Text="По:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>

    <dx:ASPxDateEdit ID="EndDateEdit" runat="server" Theme="Moderno" Font-Size="Large" Width="360px" EditFormat="DateTime">
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

    <dx:ASPxLabel ID="ASPxLabel3" runat="server" Text="Група товарів:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxComboBox ID="GrpComboBox" runat="server" Height="20px" Width="360px" Theme="Moderno" ValueField="GrpId" ValueType="System.Int32" Font-Size="Large">
        <Columns>
            <dx:ListBoxColumn Caption="Назва" FieldName="Name" />
        </Columns>
        <ButtonStyle Width="50px">
        </ButtonStyle>
    </dx:ASPxComboBox>

       <br />
    <dx:ASPxLabel ID="ASPxLabel5" runat="server" Text="Контрагент:" Font-Bold="True" Font-Size="Medium" Theme="Youthful">
    </dx:ASPxLabel>
    <dx:ASPxComboBox ID="KagentComboBox" runat="server" Width="360px" Theme="Moderno" ValueField="KaId" EnableTheming="True" Font-Size="Large" ValueType="System.Int32">
        <Columns>
            <dx:ListBoxColumn Caption="Назва" FieldName="Name" />
        </Columns>
        <ButtonStyle Width="50px">
        </ButtonStyle>
        <ValidationSettings SetFocusOnError="True" ErrorDisplayMode="None">
            <RequiredField IsRequired="True" />
        </ValidationSettings>
    </dx:ASPxComboBox>
    <br />
    <br />
    <dx:ASPxButton ID="OkButton" runat="server" Text="Сформувати" OnClick="OkButton_Click" Theme="Moderno" Width="360px" Font-Bold="False" Font-Size="Large" Height="50px">
        <FocusRectBorder BorderStyle="None" />
    </dx:ASPxButton>

    <dx:ASPxLabel ID="messageLabel" runat="server" Text="ASPxLabel" Visible="False">
    </dx:ASPxLabel>
 

</asp:Content>
