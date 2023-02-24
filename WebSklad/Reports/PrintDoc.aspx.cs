using SP.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSklad.Models;

namespace WebSklad.Reports
{
    public partial class PrintDoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var WbillId = int.Parse( HttpContext.Current.Request.Params["id"]);
            var w_type = int.Parse(HttpContext.Current.Request.Params["w_type"]);


            using (var db = Database.SPBase())
            {
                var uid = db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
                var kagent = db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);
                var wb = db.WaybillList.FirstOrDefault(w => w.WbillId == WbillId);

                var TEMPLATE = db.DocType.FirstOrDefault(w => w.Id == w_type).TemlateName;

                var print = new SP.Reports.PrintDoc();

                //      var template_name = pr2.GetTemlate(_rep_id);
                var template_file = HostingEnvironment.MapPath("~/TempLate/" + TEMPLATE);

                if (File.Exists(template_file))
                {
                    var report_data = print.CreateReport(wb.Id, w_type, template_file);
                    if (report_data != null)
                    {
                        Excel.RespondExcel(report_data, $"{wb.Num}.xlsx");
                    }
                    else
                    {
                        //      messageLabel.Visible = true;
                        //         messageLabel.Text = "За обраний період звіт не містить даних !";
                    }
                }
                else
                {
                    //    messageLabel.Visible = true;

                    //     messageLabel.Text = "Шлях до шаблонів " + template_file + " не знайдено!";
                }
            }
        }
    }
}