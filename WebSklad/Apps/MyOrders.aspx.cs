using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSklad.Apps.Views;
using WebSklad.Models;

namespace WebSklad.Apps
{
    public partial class MyOrders : CustomerPage
    {
        private int _wbill_id
        {
            get
            {
                return Convert.ToInt32(Session["WbillId"]);
            }
            set
            {
                Session["WbillId"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            /*     GridViewDataComboBoxColumn column = MyOrdersGridView.Columns["ColChecked"] as GridViewDataComboBoxColumn;
                 column.PropertiesComboBox.DataSource = new List<object>() { new { Id = 0, Name = "Новий", ImgField = "~/Img/new_order.bmp"
                 }, new { Id = 1, Name = "Відгружено", ImgField = "~/Img/execute.png" }, new { Id = 2, Name = "В обробці", ImgField = "~/Img/Частково оброблений.bmp" } };*/
        }

        protected void detailGrid_DataSelect(object sender, EventArgs e)
        {
            _wbill_id = Convert.ToInt32((sender as ASPxGridView).GetMasterRowKeyValue());

            var wbd = _db.WaybillList.Find(_wbill_id);
            if(wbd != null )
            {
                if(wbd.Checked != 0)
                {
                    (sender as ASPxGridView).Columns["CommandCol"].Visible = false;
                    (sender as ASPxGridView).SettingsDataSecurity.AllowDelete = false;
                    (sender as ASPxGridView).SettingsDataSecurity.AllowEdit = false;
                    (sender as ASPxGridView).SettingsDataSecurity.AllowInsert = false;
                }
            }

            GridViewDataComboBoxColumn column = (sender as ASPxGridView).Columns["MatNameCol"] as GridViewDataComboBoxColumn;
            column.PropertiesComboBox.DataSource = _db.Materials.Where(w=> w.TypeId == 1 || w.TypeId == 5).Select(s => new
            {
                s.MatId,
                s.Name
            }).ToList();
        }

        protected void detailGrid_CustomUnboundColumnData(object sender, ASPxGridViewColumnDataEventArgs e)
        {
            if (e.Column.FieldName == "Total")
            {
                decimal price = (decimal)e.GetListSourceFieldValue("UnitPrice");
                int quantity = Convert.ToInt32(e.GetListSourceFieldValue("Quantity"));
                e.Value = price * quantity;
            }
        }

        protected void WaybillListDS_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            e.QueryableSource = _db.WaybillList.Where(w => w.WType == -16 && (w.KaId == _ka_id || _ka_ids.Contains(w.KaId.Value))).Select(s => new MyOrdersView
            {
                WbillId = s.WbillId,
                Checked = s.Checked,
                Num = s.Num,
                OnDate = s.OnDate,
                SummAll = s.SummAll,
                Reason = s.Reason,
                Notes = s.Notes,
                KaName = s.Kagent1.Name
            });

            e.KeyExpression = "WbillId";
        }

        protected void MyOrdersGridView_HtmlRowPrepared(object sender, ASPxGridViewTableRowEventArgs e)
        {
         /*   int c1 = Convert.ToInt32(e.GetValue("Checked"));
            if (c1 == 2)
            {
                e.Row.BackColor = ColorTranslator.FromHtml("#99CCFF"); //System.Drawing.Color.fro;// #99CCFF
            }
            if (c1 == 1)
            {
                e.Row.BackColor = ColorTranslator.FromHtml("#ddf5ce");//  System.Drawing.Color.; //#cdffae
            }*/

                /*
                                  int c1 = Convert.ToInt32(e.GetValue("C1"));
                            if (e.DataColumn.FieldName != "C1") return;
                            e.Cell.Attributes.Add("fieldName", e.DataColumn.FieldName);
                            if (c1 > 0)
                                e.Cell.ForeColor = System.Drawing.Color.Orange;
                            else
                                e.Cell.ForeColor = System.Drawing.Color.Blue;*/
            }

        protected void MyOrdersGridView_HtmlDataCellPrepared(object sender, ASPxGridViewTableDataCellEventArgs e)
        {
            
        }


        protected void detailGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
        /*    var id = Convert.ToInt32(e.Keys[0]);
            
            var wbd = _db.WaybillDet.Find(id);
            if (wbd != null && wbd.WaybillList.Checked == 0)
            {
                wbd.Amount = Convert.ToDecimal(e.NewValues["Amount"]);
                _db.SaveChanges();
            }
            e.Cancel = true;*/
        }

        protected void detailGrid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            var mat_id = Convert.ToInt32(e.NewValues["MatId"]);
            var Amount = Convert.ToDecimal(e.NewValues["Amount"]);
            var price_mat_det = _db.PriceList.FirstOrDefault(w => w.PriceListKagent.Any(a => a.KagentId == _ka_id)).PriceListDet.Where(w => w.MatId == mat_id).FirstOrDefault();
            var _wb = _db.WaybillList.FirstOrDefault(w => w.WbillId == _wbill_id);

            if (_wb.Checked == 0)
            {

                var DiscountPrice = price_mat_det.Price - (price_mat_det.Price * (price_mat_det.Discount ?? 0) / 100);

                _db.WaybillDet.Add(new WaybillDet
                {
                    WbillId = _wb.WbillId,
                    Amount = Amount,
                    Discount = price_mat_det.Discount ?? 0,
                    Nds = _wb.Nds,
                    CurrId = _wb.CurrId,
                    OnDate = _wb.OnDate,
                    Num = _wb.WaybillDet.Count() + 1,
                    OnValue = _wb.OnValue,
                    PosKind = 0,
                    PosParent = 0,
                    DiscountKind = 1,
                    //   PtypeId = db.Kagent.Find(_wb.KaId).PTypeId,
                    WayBillDetAddProps = new WayBillDetAddProps(),
                    BasePrice = price_mat_det.Price,
                    Price = DiscountPrice * 100 / (100 + _wb.Nds),
                    WId = _db.Materials.Find(mat_id).WId,
                    MatId = mat_id
                });

                _db.SaveChanges();
            }

            e.Cancel = true;
        }

        protected void detailGrid_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e)
        {
            var id = Convert.ToInt32(e.Keys[0]);
            var wbd = _db.WaybillDet.Find(id);
            if (wbd == null || wbd.WaybillList.Checked != 0)
            {
                e.Cancel = true;
            }
        }

        protected void MyOrdersGridView_RowDeleting(object sender, DevExpress.Web.Data.ASPxDataDeletingEventArgs e)
        {
            var id = Convert.ToInt32(e.Keys[0]);
            var wb = _db.WaybillList.Find(id);
            if (wb != null && wb.WebUserId.Any())
            {
                _db.WaybillList.Remove(wb);
                _db.SaveChanges();
            }

            e.Cancel = true;
        }
    }
}