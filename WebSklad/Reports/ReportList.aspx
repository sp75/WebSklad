﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportList.aspx.cs" Inherits="WebSklad.Reports.ReportList" %>
<%@ Register assembly="DevExpress.Web.v21.1, Version=21.1.3.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <br />
    <dx:ASPxTreeView ID="ASPxTreeView1" runat="server" EnableTheming="True" OnVirtualModeCreateChildren="ASPxTreeView1_VirtualModeCreateChildren" EnableCallBacks="true" Theme="Moderno">
    </dx:ASPxTreeView>

</asp:Content>
