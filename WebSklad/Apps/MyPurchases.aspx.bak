<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyPurchases.aspx.cs" Inherits="WebSklad.Apps.MyPurchases" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <h2>Видаткові документи</h2>
    <p>
        <dx:ASPxGridView ID="MyOrdersGridView" runat="server" EnableTheming="True" KeyFieldName="WbillId" Theme="Moderno" Width="100%" AutoGenerateColumns="False" DataSourceID="WaybillListDS" >
          
            <SettingsPager NumericButtonCount="5" PageSize="11"/>
            <SettingsDataSecurity AllowInsert="false" AllowEdit="false" AllowDelete="false" />
            <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
            <Settings  UseFixedTableLayout="true" />
            <SettingsBehavior AllowSelectByRowClick="True" EnableRowHotTrack="True"  />
            <SettingsSearchPanel Visible="False" />
            <SettingsLoadingPanel Mode="ShowOnStatusBar" />
            <Columns>
                 <dx:GridViewDataComboBoxColumn Caption="Статус"  FieldName="Checked" VisibleIndex="0" Width="11%" AllowTextTruncationInAdaptiveMode="True">
                    <PropertiesComboBox ValueType="System.Int32">
                        <Items>
                             <dx:ListEditItem Value="1" ImageUrl="~/Img/execute.png" Text="Відгружено" />
                             <dx:ListEditItem Value="0" ImageUrl="~/Img/new_order.png" Text="Новий" />
                             <dx:ListEditItem Value="2" ImageUrl="~/Img/Частково оброблений.bmp" Text="В обробці" />
                        </Items>
                    </PropertiesComboBox>
                </dx:GridViewDataComboBoxColumn>
                <dx:GridViewDataTextColumn  Caption="#" FieldName="Num" VisibleIndex="1"  Width="8%" >
                    <DataItemTemplate>
                       <div style="white-space: nowrap;  width: auto;  overflow: hidden;  text-overflow: ellipsis; ">
                          <a href='../Reports/PrintDoc.aspx?id=<%# Container.KeyValue %>&w_type=-1' target="_self"><%#Eval("Num") %></a>
                       </div>
                    </DataItemTemplate>
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn Caption="Контрагент" FieldName="KaName" VisibleIndex="3" Width="35%" AllowTextTruncationInAdaptiveMode="True" >
                    <Settings AllowEllipsisInText="True" />
                 </dx:GridViewDataTextColumn>
                <dx:GridViewDataDateColumn Caption="Дата" FieldName="OnDate" VisibleIndex="2" Width="13%" SortIndex="0" SortOrder="Descending" >
                    <PropertiesDateEdit DisplayFormatString="g">
                    </PropertiesDateEdit>
                </dx:GridViewDataDateColumn>
                 <dx:GridViewDataTextColumn Caption="Сума" FieldName="SummAll" VisibleIndex="4" Width="10%">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn  Caption="Примітка" FieldName="Notes" VisibleIndex="5" Width="10%">
                </dx:GridViewDataTextColumn>
               
                <dx:GridViewDataTextColumn Caption="Відвантажив" FieldName="PersonName" VisibleIndex="6" Width="10%">
                </dx:GridViewDataTextColumn>



            </Columns>

            <Styles>
                <Cell Wrap="False" />
            </Styles>

                  <Templates>
            <DetailRow>
              
                <dx:ASPxGridView ID="detailGrid" runat="server" Theme="Moderno"  AutoGenerateColumns="False"
                    Width="100%"  OnBeforePerformDataSelect="detailGrid_DataSelect" OnCustomUnboundColumnData="detailGrid_CustomUnboundColumnData">
                    <SettingsSearchPanel Visible="True" />
                    <Columns>
                        <dx:GridViewDataTextColumn FieldName="Amount"  Caption="К-сть" VisibleIndex="2" Name="Quantity" Width="15%" >
                            <PropertiesTextEdit DisplayFormatString="0.00" />
                        </dx:GridViewDataTextColumn>

                        <dx:GridViewDataTextColumn FieldName="Price"  Caption="Ціна" VisibleIndex="3"  Width="15%">
                            <PropertiesTextEdit DisplayFormatString="0.00" />
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Total" Caption="Разом" VisibleIndex="4" UnboundType="Decimal" Width="20%">
                            <PropertiesTextEdit DisplayFormatString="0.00" />
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn Caption="Товар" FieldName="MatName" VisibleIndex="1" Width="40%">
                            <CellStyle Wrap="True">
                            </CellStyle>
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn Caption="#" FieldName="Num" UnboundType="Integer" Visible="False" VisibleIndex="0" Width="50px">
                        </dx:GridViewDataTextColumn>
                    </Columns>
                    <Settings ShowFooter="False"   UseFixedTableLayout="true"/>
                    <SettingsPager EnableAdaptivity="True" Mode="ShowAllRecords">
            <PageSizeItemSettings Visible="False" />
        </SettingsPager>
                    <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
                     <SettingsBehavior AllowSelectByRowClick="False" EnableRowHotTrack="True"  />
                      <Styles>
                          <Cell Wrap="False" />
                      </Styles>
                    <TotalSummary>
                        <dx:ASPxSummaryItem FieldName="CompanyName" SummaryType="Count" />
                        <dx:ASPxSummaryItem FieldName="Total" SummaryType="Sum" />
                        <dx:ASPxSummaryItem FieldName="Quantity" SummaryType="Sum" />
                    </TotalSummary>
                </dx:ASPxGridView>
            </DetailRow>
        </Templates>
        <SettingsDetail ShowDetailRow="true" AllowOnlyOneMasterRowExpanded="True" />
        </dx:ASPxGridView>

       <dx:EntityServerModeDataSource ID="WaybillListDS" runat="server" ContextTypeName="SP.Base.Models.SPBaseModel" TableName="WaybillList" OnSelecting="WaybillListDS_Selecting"             />
  
    </p>
   
</asp:Content>
