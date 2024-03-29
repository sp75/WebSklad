﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ShopSettings.aspx.cs" Inherits="WebSklad.Apps.Tranzit.ShopSettings" %>
<%@ Register assembly="DevExpress.Web.v21.1, Version=21.1.3.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v21.1, Version=21.1.3.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
         <h2>Налаштування магазинів</h2>
    <p>
        <dx:ASPxGridView ID="MyOrdersGridView" runat="server" EnableTheming="True" KeyFieldName="KaId" Theme="Moderno" Width="100%" AutoGenerateColumns="False" DataSourceID="WaybillListDS" OnRowUpdating="MyOrdersGridView_RowUpdating" >
          
            <SettingsPager NumericButtonCount="5" PageSize="11"/>
            <SettingsDataSecurity AllowInsert="false" AllowDelete="false" />
            <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
            <Settings  UseFixedTableLayout="true" />
            <SettingsBehavior AllowSelectByRowClick="True" EnableRowHotTrack="True"  />
            <SettingsSearchPanel Visible="true" />
            <SettingsLoadingPanel Mode="ShowOnStatusBar" />
            <Columns>
<dx:GridViewDataTextColumn FieldName="KaId" VisibleIndex="1" Width="8%" Caption="#" ShowInCustomizationForm="True">
    <EditFormSettings Visible="False" />
</dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn FieldName="Name" VisibleIndex="2" Caption="Назва" ReadOnly="True" Width="60%" >
                </dx:GridViewDataTextColumn>



                <dx:GridViewDataCheckColumn FieldName="is_exist" VisibleIndex="3" Caption="Імпорт" Width="10%">
                </dx:GridViewDataCheckColumn>



                <dx:GridViewCommandColumn ShowEditButton="True" VisibleIndex="0" Width="8%">
                </dx:GridViewCommandColumn>



            </Columns>

            <Styles>
                <Cell Wrap="False" />
            </Styles>


        </dx:ASPxGridView>

       <dx:EntityServerModeDataSource ID="WaybillListDS" runat="server" ContextTypeName="SP.Base.Models.SPBaseModel" TableName="Kagent" OnSelecting="WaybillListDS_Selecting" EnableUpdate="True"             />
  
    </p>
</asp:Content>
