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
    public partial class CustomPrintDoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var WbillId = int.Parse(HttpContext.Current.Request.Params["id"]);
            var price_name = HttpContext.Current.Request.Params["price"];
            var template = HttpContext.Current.Request.Params["template"];

            using (var db = SPDatabase.SPBase())
            {
                var uid = db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
                var kagent = db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);
                var wb = db.WaybillList.FirstOrDefault(w => w.WbillId == WbillId);

                var pl_id = db.PriceList.FirstOrDefault(w => w.Name == price_name)?.PlId;

               var TEMPLATE = string.IsNullOrEmpty(template) ? "WayBill_Out_Custom.xlsx" : template+ ".xlsx";// db.DocType.FirstOrDefault(w => w.Id == -1).TemlateName;

                var print = new SP.Reports.PrintDoc();

                var template_file = HostingEnvironment.MapPath("~/TempLate/" + TEMPLATE);

                if (File.Exists(template_file))
                {
                    var report_data = print.CreateCustomReport(wb.Id, template_file, pl_id ?? 0);
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