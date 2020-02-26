using SP.Base;
using SP.Reports;
using SP.Reports.Models.Views;
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
    public partial class rep6 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (var db = Database.SPBase())
            {
                Label1.Text = db.RepLng.First(w => w.RepId == 6 && w.LangId == 2).Name;
            }

            if (!IsPostBack)
            {
                OnDateDBEdit.Date = DateTime.Now;
            }
        }


        protected void OkButton_Click(object sender, EventArgs e)
        {
            int _rep_id = 6;

            using (var db = Database.SPBase())
            {
                var TEMPLATE = db.Reports.FirstOrDefault(w => w.RepId == _rep_id).TemlateName;

                var uid = db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
                var kagent = db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);

                var pr2 = new PrintReportv2(_rep_id, kagent.KaId, kagent.UserId.Value)
                {
                    OnDate = OnDateDBEdit.Date,
                    //    StartDate = StartDateEdit.DateTime,
                    //     EndDate = EndDateEdit.DateTime,
                 //   MatGroup = new GrpComboBoxItem { GrpId = Convert.ToInt32(GrpComboBox.SelectedItem.GetValue("GrpId")), Name = GrpComboBox.SelectedItem.GetValue("Name").ToString() },
                    // Kagent = KagentComboBox.GetSelectedDataRow(), 
               //     Warehouse = new WhComboBoxItem { WId = WhComboBox.SelectedItem.GetValue("WId").ToString(), Name = WhComboBox.SelectedItem.GetValue("Name").ToString() },
                    //       Material = MatComboBox.GetSelectedDataRow(),
                    //        DocStr = str,
                    //       DocType = DocTypeEdit.EditValue,
                    //        ChType = ChTypeEdit.GetSelectedDataRow(),
                    //       Status = wbStatusList.EditValue,
                    //        KontragentGroup = GrpKagentLookUpEdit.GetSelectedDataRow(),
                    //      GrpStr = ChildGroupCheckEdit.Checked ? String.Join(",", new BaseEntities().GetMatGroupTree(grp).ToList().Select(s => Convert.ToString(s.GrpId))) : "",
                    //       Person = PersonLookUpEdit.GetSelectedDataRow()
                };

                //      var template_name = pr2.GetTemlate(_rep_id);
                var template_file = HostingEnvironment.MapPath("~/TempLate/" + TEMPLATE);

                if (File.Exists(template_file))
                {
                    var report_data = pr2.CreateReport(template_file, "pdf");  //xlsx , pdf
                    if (report_data != null)
                    {
                        //     Excel.RespondExcel(report_data, "RepMatRest.xlsx");
                        Pdf.RespondPdf(report_data, TEMPLATE);
                    }
                    else
                    {
                        messageLabel.Visible = true;
                        messageLabel.Text = "За обраний період звіт не містить даних !";
                    }
                }
                else
                {
                    messageLabel.Visible = true;

                    messageLabel.Text = "Шлях до шаблонів " + template_file + " не знайдено!";
                }
            }

        }
    }
}