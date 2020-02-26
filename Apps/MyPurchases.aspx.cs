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


namespace WebSklad.Apps
{
    public partial class MyPurchases : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GridViewDataComboBoxColumn column = ASPxGridView1.Columns["ColChecked"] as GridViewDataComboBoxColumn;
            column.PropertiesComboBox.DataSource = new List<object>() { new { Id = 0, Name = "Не проведений" }, new { Id = 1, Name = "Виконаний" } };

            using (var db = Database.SPBase())
            {
                var uid = db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
                var kagent = db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);

                ASPxGridView1.DataSource = WhMatGet(DateTime.Now.AddYears(-100), DateTime.Now.Date.AddDays(1), -1, -1, kagent.KaId, 1, "*", 1028);
                ASPxGridView1.DataBind();
            }
        }

        public List<GetWayBillList_Result> WhMatGet(DateTime satrt_date, DateTime end_dated, int wtyp, int status, int? ka_id, int show_null_balance, string wh, int? user_id)
        {
            using (var db = Database.SPBase())
            {
                return db.Database.SqlQuery<GetWayBillList_Result>("select * from GetWayBillList({0}, {1}, {2}, {3} , {4}, {5}, {6}, {7})", satrt_date, end_dated, wtyp, status, ka_id, show_null_balance, wh, user_id).OrderByDescending(o => o.OnDate).ToList();
            }
        }


    }
}