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
    public partial class ProductSettings : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void WaybillListDS_Selecting(object sender, DevExpress.Data.Linq.LinqServerModeDataSourceSelectEventArgs e)
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var st = db.Product.Select(s => s.Id).ToList();

                e.QueryableSource = _db.Materials.Where(w => w.Deleted == 0).Select(s => new ProductList
                {
                    MatId = s.MatId,
                    Name = s.Name,
                    is_exist = st.Contains(s.MatId),
                    GroupName = s.MatGroup.Name,
                    Archived = s.Archived == 1
                });

                e.KeyExpression = "MatId";
            }
        }

        public class ProductList
        {
            public int MatId { get; set; }
            public String Name { get; set; }
            public bool is_exist { get; set; }
            public string GroupName { get; set; }
            public bool Archived { get; set; }
        }


        protected void MyOrdersGridView_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            using (var db = new Tranzit_Waybills_OSEntities())
            {
                var new_is_exist = Convert.ToBoolean(e.NewValues["is_exist"]);
                var key = Convert.ToInt32(e.Keys[0]);


                if (!new_is_exist)
                {
                    if (db.Product.Any(a => a.Id == key))
                    {
                        db.Product.Remove(db.Product.Find(key));
                    }
                }
                else
                {
                    if (!db.Product.Any(a => a.Id == key))
                    {
                        var mat = _db.Materials.FirstOrDefault(w => w.MatId == key);
                        db.Product.Add(new Product
                        {
                            Id = key,
                            Name = mat.Name,
                            Artikul = mat.Artikul
                        });
                    }
                }

                db.SaveChanges();
            }
            
            e.Cancel=true;
            MyOrdersGridView.CancelEdit();
        }

    }
}