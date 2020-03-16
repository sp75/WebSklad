using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSklad.Models;

namespace WebSklad.Apps
{
    public partial class PriceList : CustomerPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void PriceListDetDS_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            e.QueryableSource = _db.PriceListDet.Where(w => w.PlId == 205).Select(s => new
            {
                s.PlDetId,
                s.Num,
                s.Price,
                s.Discount,
                MatMeasures = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Measures.ShortName,
                MatName = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Name,
                MatImg = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).BMP,
                MatNotes = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Notes,
            });

            e.KeyExpression = "PlDetId";
        }
    }
}