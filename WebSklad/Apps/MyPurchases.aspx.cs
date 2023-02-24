using DevExpress.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SP.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSklad.Models;

namespace WebSklad.Apps
{
    public partial class MyPurchases : CustomerPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
      
        }

        protected void detailGrid_DataSelect(object sender, EventArgs e)
        {
            var WbillId = Convert.ToInt32((sender as ASPxGridView).GetMasterRowKeyValue());

            ((ASPxGridView)sender).DataSource = _db.WaybillDet.Where(w => w.WbillId == WbillId).Select(s => new
            {
                s.PosId,
                s.Num,
                s.Price,
                s.Total,
                MatName = s.Materials.Name,
                s.Amount
            }).ToList();
        }
        protected void WaybillListDS_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            e.QueryableSource = _db.WaybillList.Where(w => w.WType == -1 && (w.KaId == _ka_id || _ka_ids.Contains(w.KaId.Value))).Select(s => new
            {
                s.WbillId,
                s.Checked,
                s.Num,
                s.OnDate,
                s.SummAll,
                s.Reason,
                s.Notes,
                PersonName = s.Kagent.Name,
                KaName = s.Kagent1.Name
            });

            e.KeyExpression = "WbillId";
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
    }
}