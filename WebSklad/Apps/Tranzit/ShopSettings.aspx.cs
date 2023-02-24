using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tranzit_Waybills_OS_DB;
using WebSklad.Models;

namespace WebSklad.Apps.Tranzit
{
    public partial class ShopSettings : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void WaybillListDS_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var st = db.Shop.Select(s => s.Id).ToList();

                e.QueryableSource = _db.Kagent.Where(w => w.Archived == null || w.Archived == 0).Select(s => new ShopList
                {
                    KaId = s.KaId,
                    Name = s.Name,
                    is_exist = st.Contains(s.KaId)
                });

                e.KeyExpression = "KaId";
            }
        }

        public class ShopList
        {
            public int KaId { get; set; }
            public String Name { get; set; }
            public bool is_exist { get; set; }
        }


        protected void MyOrdersGridView_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var new_is_exist = Convert.ToBoolean(e.NewValues["is_exist"]);
                var key = Convert.ToInt32(e.Keys[0]);


                if (!new_is_exist)
                {
                    if (db.Shop.Any(a => a.Id == key))
                    {
                        db.Shop.Remove(db.Shop.Find(key));
                    }
                }
                else
                {
                    if (!db.Shop.Any(a => a.Id == key))
                    {
                        var kagent = _db.Kagent.FirstOrDefault(w => w.KaId == key);
                        db.Shop.Add(new Shop { Id = key, Name = kagent.Name });
                    }
                }

                db.SaveChanges();
            }
            
            e.Cancel=true;
            MyOrdersGridView.CancelEdit();
        }

    }
}