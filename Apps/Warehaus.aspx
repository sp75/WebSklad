<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Warehaus.aspx.cs" Inherits="WebSklad.Apps.Warehaus" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <p>
        <dx:ASPxGridView ID="ASPxGridView1" runat="server" EnableTheming="True" KeyFieldName="MatId" Theme="SoftOrange" Width="1341px">
            <SettingsPager  PageSize="50">
            </SettingsPager>
            <Settings ShowGroupPanel="True" />
            <SettingsBehavior AllowSelectByRowClick="True" EnableRowHotTrack="True" />
            <SettingsCookies CookiesID="wh_grid" Enabled="True" Version="1.0.0.0" />
            <SettingsDataSecurity AllowDelete="False" AllowEdit="False" AllowInsert="False" />
  
        </dx:ASPxGridView>
    </p>
</asp:Content>
