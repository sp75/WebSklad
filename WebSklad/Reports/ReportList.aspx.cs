using DevExpress.Web;
using SP.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebSklad.Models;

namespace WebSklad.Reports
{
    public partial class ReportList : CustomerPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ASPxTreeView1_VirtualModeCreateChildren(object source, DevExpress.Web.TreeViewVirtualModeCreateChildrenEventArgs e)
        {
            using (var db = SPDatabase.SPBase())
            {
                List<TreeViewVirtualNode> children = new List<TreeViewVirtualNode>();

                var list = db.Reports.ToList().Select(s => new
                {
                    Id = s.RepId*100,
                    ParentID = s.GrpId,
                    Num = s.Num,
                    Name = db.RepLng.FirstOrDefault(w => w.LangId == 2 && w.RepId == s.RepId).Name,
                    HasChilds = false
                }).Concat(db.UserTreeAccess.Where(w => w.UserId == _user_id && w.PId == 34).ToList().Select(s => new
                {
                    Id = s.Id,
                    ParentID = 0,
                    Num = (int?)null,
                    Name = s.Name,
                    HasChilds = true
                })).ToList();


                foreach (var item in list)
                {
                    string parentName = e.NodeName != null ? e.NodeName.ToString() : "0";
                    if (item.ParentID.ToString() == parentName)
                    {
                        TreeViewVirtualNode child = new TreeViewVirtualNode( item.Id.ToString(), item.Name);
                        if (item.Num.HasValue)
                        {
                            child.Text = item.Num.ToString() + ". " + item.Name;
                            child.NavigateUrl = "~/Reports/rep" + item.Num.ToString() + ".aspx";
                        }
                        children.Add(child);
                        child.IsLeaf = !item.HasChilds;
                    }
                    e.Children = children;
                }
            }

        }

    }
}