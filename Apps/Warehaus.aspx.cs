using SP.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace WebSklad.Apps
{
    public partial class Warehaus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ASPxGridView1.DataSource = WhMatGet(0, 0, 0, DateTime.Now, 0, "*", 0, "", 0, 0);
            ASPxGridView1.DataBind();
        }

        public List<WhMatGet_Result> WhMatGet(int? grp_id, int? wid, int? ka_id, DateTime? on_date, int? get_empty, string wh, int? show_all_mats, string grp, int? user_id, int? get_child_node)
        {
            using (var db = Database.SPBase())
            {
                return db.Database.SqlQuery<WhMatGet_Result>("select * from WhMatGet(0, 0, 0,  GETDATE() , 0, '*', 0, '', 0, 0)", grp_id, wid, ka_id, on_date, get_empty, wh, show_all_mats, grp, user_id, get_child_node).ToList();
            }
        }
    }
}