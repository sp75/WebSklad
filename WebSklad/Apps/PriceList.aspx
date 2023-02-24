<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PriceList.aspx.cs" Inherits="WebSklad.Apps.PriceList" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>
<%@ Register assembly="DevExpress.Web.v15.2, Version=15.2.16.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Data.Linq" tagprefix="dx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        function AmountChanged(s, e) {
            var p = parseFloat(s.GetValue()) + '|' + parseFloat(e);
            PriceListGrid.PerformCallback(p);
        }
    </script>

    <style type="text/css">
        .options .options-item {
            display: inline-block;
            vertical-align: top;
            padding: 3px 15px 3px 0;
        }
    </style>

    <p>
        <dx:ASPxGridView ID="PriceListGrid" runat="server" DataSourceID="PriceListDetDS" Width="100%" Theme="Moderno"
            KeyFieldName="PlDetId" PreviewFieldName="MatNotes" EnableRowsCache="False" AutoGenerateColumns="False" ClientInstanceName="PriceListGrid" OnCustomCallback="PriceListGrid_CustomCallback">
            <Columns>
                <dx:GridViewDataColumn FieldName="MatName" VisibleIndex="1" Caption="Товар" />
                <dx:GridViewDataColumn FieldName="MatMeasures" VisibleIndex="2" Caption="Од." />
                <dx:GridViewDataColumn FieldName="Discount" VisibleIndex="5" Caption="Знижка" />
                <dx:GridViewDataTextColumn Caption="Ціна" FieldName="Price" VisibleIndex="4">
                    <PropertiesTextEdit DisplayFormatString="0.00" />
                </dx:GridViewDataTextColumn>
            </Columns>
            <Templates>
                <PreviewRow>
                    <table>
                        <tr>
                            <td style="padding-right: 12px">
                                <dx:ASPxBinaryImage ID="ASPxBinaryImage1" runat="server" Value='<%# Eval("MatImg") %>' Height="150px" Theme="Moderno" ImageSizeMode="ActualSizeOrFit" Width="150" />
                            </td>
                            <td style="vertical-align: top">
                                <dx:ASPxLabel ID="lblNotes" runat="server" Text='<%# Eval("MatNotes") %>' Theme="Moderno" Height="100px" />

                                <div class="options">
                                    <div class="options-item">
                                        <dx:ASPxSpinEdit ID="ASPxSpinEdit1" runat="server" Number='<%# Eval("Amount") %>' MaxValue="10000" Theme="Moderno" Width="148px" Height="40px" HorizontalAlign="Center" Font-Size="12pt" DecimalPlaces="2" ClientInstanceName="AmountEdit" ClientSideEvents-ButtonClick='<%# "function(s, e) { AmountChanged(s, " + Container.KeyValue + "); }" %>' AllowMouseWheel="False">
                                            <SpinButtons ShowIncrementButtons="False">
                                            </SpinButtons>
                                            <ClientSideEvents GotFocus="function(s, e) {
	s.SelectAll();
}" />
                                            <Buttons>
                                                <dx:EditButton>
                                                    <Image IconID="actions_buy_32x32devav">
                                                    </Image>
                                                </dx:EditButton>
                                            </Buttons>
                                        </dx:ASPxSpinEdit>
                                    </div>
                                    <div class="options-item">
                                    </div>
                                </div>


                            </td>
                        </tr>
                    </table>
                </PreviewRow>
            </Templates>
            <Settings ShowPreview="true" />
            <SettingsSearchPanel Visible="True" />
            <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" />
        </dx:ASPxGridView>
        <dx:EntityServerModeDataSource ID="PriceListDetDS" runat="server" ContextTypeName="SP.Base.Models.SPBaseModel" TableName="PriceListDet" OnSelecting="PriceListDetDS_Selecting" />
    </p>
</asp:Content>
