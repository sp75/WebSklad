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
    public partial class rep3 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (var db = Database.SPBase())
            {
                Label1.Text = db.RepLng.First(w => w.RepId == 3 && w.LangId == 2).Name;

                var wh = db.Warehouse.Where(w => w.UserAccessWh.Any(a => a.UserId == 0)).Select(s => new { WId = s.WId.ToString(), s.Name, s.Def }).ToList();

                WhComboBox.DataSource = new List<WhComboBoxItem>() { new WhComboBoxItem { WId = "*", Name = "Усі" } }.Concat(wh.Select(s => new WhComboBoxItem
                {
                    WId = s.WId,
                    Name = s.Name
                }).ToList());
                WhComboBox.DataBind();
             
                GrpComboBox.DataSource = new List<GrpComboBoxItem>() { new GrpComboBoxItem { GrpId = 0, Name = "Усі" } }.Concat(db.MatGroup.Where(w => w.Deleted == 0).Select(s => new GrpComboBoxItem { GrpId = s.GrpId, Name = s.Name }).ToList());
                GrpComboBox.DataBind();

                var k_list = db.Kagent.Where(w=> w.Deleted == 0 && (!w.Archived.HasValue || w.Archived == 0)).OrderBy(o => o.Name).Select(s => new KagentComboBoxItem { KaId = s.KaId, Name = s.Name }).ToList();
                KagentComboBox.DataSource = k_list;
                KagentComboBox.DataBind();

                if (!IsPostBack)
                {
                    StartDateEdit.Date = DateTime.Now.Date.AddDays(-1);
                    EndDateEdit.Date = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    WhComboBox.Value = "*";
                    GrpComboBox.Value = 0;
                //    KagentComboBox.Value =  k_list.FirstOrDefault().KaId;
                }

            }

          

        }


        protected void OkButton_Click(object sender, EventArgs e)
        {
            int _rep_id = 3;

            using (var db = Database.SPBase())
            {
                var uid = db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
                var kagent = db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);
                var TEMPLATE = db.Reports.FirstOrDefault(w => w.RepId == _rep_id).TemlateName;



                var pr2 = new PrintReportv2(_rep_id, kagent.KaId, kagent.UserId.Value)
                {
                    // OnDate = OnDateDBEdit.Date,
                    StartDate = StartDateEdit.Date,
                    EndDate = EndDateEdit.Date,
                    MatGroup = new GrpComboBoxItem { GrpId = Convert.ToInt32(GrpComboBox.SelectedItem.GetValue("GrpId")), Name = GrpComboBox.SelectedItem.GetValue("Name").ToString() },
                    Kagent = new KagentComboBoxItem { KaId = Convert.ToInt32(KagentComboBox.SelectedItem.GetValue("KaId")), Name = KagentComboBox.SelectedItem.GetValue("Name").ToString() } , 
                    Warehouse = new WhComboBoxItem { WId = WhComboBox.SelectedItem.GetValue("WId").ToString(), Name = WhComboBox.SelectedItem.GetValue("Name").ToString() },
                    //       Material = MatComboBox.GetSelectedDataRow(),
                    //        DocStr = str,
                    //       DocType = DocTypeEdit.EditValue,
                    //        ChType = ChTypeEdit.GetSelectedDataRow(),
                    //       Status = wbStatusList.EditValue,
                     KontragentGroup = new GrpKagentComboBoxItem {  Id = Guid.Empty, Name = "Усі"},
                    //      GrpStr = ChildGroupCheckEdit.Checked ? String.Join(",", new BaseEntities().GetMatGroupTree(grp).ToList().Select(s => Convert.ToString(s.GrpId))) : "",
                    //       Person = PersonLookUpEdit.GetSelectedDataRow()
                };

                //      var template_name = pr2.GetTemlate(_rep_id);
                var template_file = HostingEnvironment.MapPath("~/TempLate/"+ TEMPLATE);

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