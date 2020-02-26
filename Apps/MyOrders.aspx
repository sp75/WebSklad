<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyOrders.aspx.cs" Inherits="WebApplication1.Apps.MyOrders" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

        <p>
        <dx:ASPxGridView ID="ASPxGridView1" runat="server" EnableTheming="True" KeyFieldName="MatId" Theme="SoftOrange" Width="1341px" AutoGenerateColumns="False">
            <SettingsPager PageSize="30">
            </SettingsPager>
            <Settings ShowFilterRow="True" ShowFilterRowMenu="True" />
            <SettingsBehavior AllowSelectByRowClick="True" EnableRowHotTrack="True" ColumnResizeMode="NextColumn" />
            <SettingsCookies CookiesID="my_oreders_grid" Enabled="True" Version="1.0.0.0" StoreColumnsVisiblePosition="False" />
            <SettingsDataSecurity AllowDelete="False" AllowEdit="False" AllowInsert="False" />
            <Columns>
                <dx:GridViewDataTextColumn Caption="#" FieldName="Num" VisibleIndex="0" Width="80px">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataDateColumn Caption="Дата" FieldName="OnDate" VisibleIndex="1" Width="130px">
                    <PropertiesDateEdit DisplayFormatString="g" EditFormat="DateTime">
                    </PropertiesDateEdit>
                </dx:GridViewDataDateColumn>
                <dx:GridViewDataComboBoxColumn Caption="Статус" FieldName="Checked" VisibleIndex="3" Name="ColChecked">
                    <PropertiesComboBox TextField="Name" ValueField="Id">
                    </PropertiesComboBox>
                </dx:GridViewDataComboBoxColumn>
                <dx:GridViewDataTextColumn Caption="Сума" FieldName="SummAll" VisibleIndex="4">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn Caption="Прийняв замовлення" FieldName="PersonName" VisibleIndex="5">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn Caption="Контрагент" FieldName="KaName" VisibleIndex="2">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn Caption="Підприємство" FieldName="EntName" VisibleIndex="6">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn Caption="Примітка" FieldName="Notes" VisibleIndex="7">
                </dx:GridViewDataTextColumn>
            </Columns>
        </dx:ASPxGridView>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
    </p>
</asp:Content>
