<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportList.aspx.cs" Inherits="WebApplication1.Reports.ReportList" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxTreeView ID="ASPxTreeView1" runat="server" EnableTheming="True" OnVirtualModeCreateChildren="ASPxTreeView1_VirtualModeCreateChildren" EnableCallBacks="true" Theme="Moderno">
    </dx:ASPxTreeView>

</asp:Content>
