<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyOrders.aspx.cs" Inherits="WebSklad.Apps.MyOrders" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
       <h2>Мої замовлення</h2>
        <p>
        <dx:ASPxGridView ID="MyOrdersGridView" runat="server" EnableTheming="True" KeyFieldName="WbillId" Theme="Moderno" Width="100%" AutoGenerateColumns="False" DataSourceID="WaybillListDS" OnHtmlRowPrepared="MyOrdersGridView_HtmlRowPrepared" OnRowDeleting="MyOrdersGridView_RowDeleting">
          
            <SettingsPager NumericButtonCount="5" PageSize="11"/>
            <SettingsDataSecurity AllowInsert="false" AllowEdit="false" />
            <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
            <Settings  UseFixedTableLayout="true" />
          
            <SettingsBehavior AllowSelectByRowClick="True" EnableRowHotTrack="True"  />
  
            <SettingsLoadingPanel Mode="ShowOnStatusBar" />
             <SettingsCommandButton>
                        <NewButton ButtonType="Image">
                            <Image IconID="actions_add_16x16">
                            </Image>
                        </NewButton>
                       
                       
                        <DeleteButton ButtonType="Image" Image-Url="../Img/delete.png">
                            <Image>
                            </Image>
                        </DeleteButton>
                    </SettingsCommandButton>

            <Columns>
                <dx:GridViewCommandColumn ButtonType="Image" ShowDeleteButton="True" VisibleIndex="6" Width="5%" AdaptivePriority="1">
                </dx:GridViewCommandColumn>
                <dx:GridViewDataTextColumn Caption="Номер" FieldName="Num" VisibleIndex="1" Width="8%">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataDateColumn Caption="Дата" FieldName="OnDate" VisibleIndex="2" Width="11%" SortIndex="0" SortOrder="Descending">
                    <PropertiesDateEdit DisplayFormatString="dd-MM-yyyy">
                    </PropertiesDateEdit>
                </dx:GridViewDataDateColumn>
                 <dx:GridViewDataTextColumn Caption="Сума" FieldName="SummAll" VisibleIndex="4" Width="8%">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataTextColumn  Caption="Примітка" FieldName="Notes" VisibleIndex="5" Width="12%" AdaptivePriority="1">
                </dx:GridViewDataTextColumn>
                <dx:GridViewDataComboBoxColumn Caption="Статус"  FieldName="Checked" VisibleIndex="0"  Width="11%">
                    <PropertiesComboBox ValueType="System.Int32">
                        <Items>
                             <dx:ListEditItem Value="1" ImageUrl="~/Img/execute.png" Text="Відгружено" />
                             <dx:ListEditItem Value="0" ImageUrl="~/Img/new_order.png" Text="Новий" />
                             <dx:ListEditItem Value="2" ImageUrl="~/Img/Частково оброблений.bmp" Text="В обробці" />
                        </Items>
                    </PropertiesComboBox>
                </dx:GridViewDataComboBoxColumn>
                <dx:GridViewDataTextColumn Caption="Контрагент" FieldName="KaName" VisibleIndex="3"  Width="40%" AdaptivePriority="0"  AllowTextTruncationInAdaptiveMode="True">
                </dx:GridViewDataTextColumn>
            </Columns>

            <Styles>
                <Cell Wrap="False" />
            </Styles>

                  <Templates>
            <DetailRow>
              
                <dx:ASPxGridView ID="detailGrid" runat="server" Theme="Moderno"  AutoGenerateColumns="False" KeyFieldName="PosId"
                    Width="100%"  OnBeforePerformDataSelect="detailGrid_DataSelect" DataSourceID="WaybillDetDS" OnRowUpdating="detailGrid_RowUpdating" OnRowDeleting="detailGrid_RowDeleting" OnRowInserting="detailGrid_RowInserting">

                    <SettingsCommandButton>
                        <NewButton ButtonType="Image">
                            <Image IconID="actions_add_16x16">
                            </Image>
                        </NewButton>
                       
                       
                        <DeleteButton ButtonType="Image" Image-Url="../Img/delete.png">
                            <Image>
                            </Image>
                        </DeleteButton>
                    </SettingsCommandButton>

                    <SettingsSearchPanel Visible="True" />
                    <Columns>
                        <dx:GridViewDataTextColumn FieldName="Price"  Caption="Ціна" VisibleIndex="3" AdaptivePriority="1" Width="10%">
                            <PropertiesTextEdit DisplayFormatString="0.00" />
                            <EditFormSettings Visible="False" />
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataTextColumn FieldName="Total" Caption="Разом" VisibleIndex="4" UnboundType="Decimal" ReadOnly="True" AdaptivePriority="2" Width="10%">
                            <PropertiesTextEdit DisplayFormatString="0.00" />
                            <EditFormSettings Visible="False" />
                        </dx:GridViewDataTextColumn>
                        <dx:GridViewDataSpinEditColumn Caption="К-сть" FieldName="Amount" Name="AmountCol" UnboundType="Decimal" VisibleIndex="2" Width="12%">
                            <PropertiesSpinEdit DisplayFormatString="0.00" NumberFormat="Custom" AllowMouseWheel="False" AllowNull="False" MaxValue="10000">
                                <SpinButtons ShowIncrementButtons="False">
                                </SpinButtons>
                                <ValidationSettings Display="None" ErrorDisplayMode="None">
                                </ValidationSettings>
                            </PropertiesSpinEdit>

                        </dx:GridViewDataSpinEditColumn>
                        <dx:GridViewCommandColumn ShowDeleteButton="True" ShowNewButtonInHeader="True" VisibleIndex="0" Width="25px" Name="CommandCol">
                        </dx:GridViewCommandColumn>
                        <dx:GridViewDataComboBoxColumn Caption="Товар" FieldName="MatId" VisibleIndex="1" Width="30%" Name="MatNameCol">
                            <PropertiesComboBox TextField="Name" ValueField="MatId" ValueType="System.Int32">
                            </PropertiesComboBox>
                            <CellStyle Wrap="True">
                            </CellStyle>
                        </dx:GridViewDataComboBoxColumn>
                    </Columns>
                    <SettingsPager Mode="ShowAllRecords">
                    </SettingsPager>
                    <SettingsEditing Mode="Batch">
                        <BatchEditSettings AllowValidationOnEndEdit="False" ShowConfirmOnLosingChanges="True" />
                    </SettingsEditing>
                    <Settings ShowFooter="False"   UseFixedTableLayout="true"/>
                    <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
                     <SettingsBehavior AllowSelectByRowClick="False" EnableRowHotTrack="True" ConfirmDelete="True"  />
                      <Styles>
                          <Cell Wrap="True" />
                      </Styles>
                </dx:ASPxGridView>
            </DetailRow>
        </Templates>
        <SettingsDetail ShowDetailRow="True" AllowOnlyOneMasterRowExpanded="True" />
        </dx:ASPxGridView>

       <dx:EntityServerModeDataSource ID="WaybillListDS" runat="server" ContextTypeName="SP.Base.Models.SPBaseModel" TableName="WaybillList" OnSelecting="WaybillListDS_Selecting"/>
       <ef:EntityDataSource runat="server" ID="WaybillDetDS" ContextTypeName="SP.Base.Models.SPBaseModel" EntitySetName="WaybillDet" Where="it.WbillId = @WbillId" EnableDelete="True" EnableUpdate="True">
         <WhereParameters>
            <asp:SessionParameter Name="WbillId" SessionField="WbillId" Type="Int32" />
         </WhereParameters>
       </ef:EntityDataSource>
        
        </p>
</asp:Content>
