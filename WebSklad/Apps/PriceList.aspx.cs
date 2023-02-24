using DevExpress.Web;
using SkladEngine.Common;
using SP.Base.Models;
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
            var pl = _db.PriceList.Where(w => w.Kagent.Any(a => a.KaId == _ka_id)).FirstOrDefault();
            var pl_id = pl != null ? pl.PlId : 0;
            var date = DateTime.Now.Date;

            e.QueryableSource = _db.PriceListDet.Where(w => w.PlId == pl_id).Select(s => new
            {
                s.PlDetId,
                s.Num,
                s.Price,
                s.Discount,
                MatMeasures = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Measures.ShortName,
                MatName = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Name,
                MatImg = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).BMP,
                MatNotes = _db.Materials.FirstOrDefault(w => w.MatId == s.MatId).Notes,
                Amount = _db.WaybillDet.Where(w=> w.MatId == s.MatId && w.WaybillList.WType == -16 && w.WaybillList.Checked == 0 && w.WaybillList.KaId == _ka_id && w.WaybillList.OnDate > date).OrderByDescending(o => o.OnDate).Select(s1=> s1.Amount).FirstOrDefault()
            });

            e.KeyExpression = "PlDetId";
        }

        protected void ASPxSpinEdit1_ValueChanged(object sender, EventArgs e)
        {
            var se = sender as ASPxSpinEdit;
            var date = DateTime.Now.Date;

            var PlDetId = Convert.ToInt32( (se.Parent as GridViewPreviewRowTemplateContainer).KeyValue  );

         

        }

        protected void PriceListGrid_CustomCallback(object sender, ASPxGridViewCustomCallbackEventArgs e)
        {
            var parameters = Convert.ToString(e.Parameters).Split('|');
            if(parameters.Count() != 2)
            {
                return;
            }

            var PlDetId = Convert.ToInt32(parameters[1]);
            var amount = Convert.ToDecimal(parameters[0]);
            var date = DateTime.Now.Date;


            var pld = _db.PriceListDet.Where(w => w.PlDetId == PlDetId).FirstOrDefault();

            if (pld != null)
            {
                var wb = _db.WaybillList.Where(w => w.WType == -16 && w.Checked == 0 && w.KaId == _ka_id && w.OnDate > date).OrderByDescending(o => o.OnDate).FirstOrDefault();

                if (wb == null)
                {
                    wb = _db.WaybillList.Add(new WaybillList()
                    {
                        Id = Guid.NewGuid(),
                        WType = -16,
                        OnDate = DBHelper.ServerDateTime(),
                        Num = _db.GetDocNum("wb(-16)").FirstOrDefault(),
                        CurrId = 2,
                        OnValue = 1,
                        PersonId = _ka_id,
                        EntId = DBHelper.EnterpriseList(_ka_id).Select(s => s.KaId).FirstOrDefault(),
                        UpdatedBy = _user_id,
                        Nds = 0,
                        KaId = _ka_id,
                        WebUserId = _db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id

                    });
                    _db.SaveChanges();
                }

                var wbd = wb.WaybillDet.Where(w => w.MatId == pld.MatId).OrderByDescending(o => o.OnDate).FirstOrDefault();
                if (wbd != null)
                {
                    wbd.Amount = amount;
                }
                else
                {
                    var DiscountPrice = pld.Price - (pld.Price * (pld.Discount ?? 0) / 100);

                    _db.WaybillDet.Add(new WaybillDet
                    {
                        WbillId = wb.WbillId,
                        Amount = amount,
                        Discount = pld.Discount ?? 0,
                        Nds = wb.Nds,
                        CurrId = wb.CurrId,
                        OnDate = wb.OnDate,
                        Num = wb.WaybillDet.Count() + 1,
                        OnValue = wb.OnValue,
                        PosKind = 0,
                        PosParent = 0,
                        DiscountKind = 1,
                        //   PtypeId = db.Kagent.Find(_wb.KaId).PTypeId,
                        WayBillDetAddProps = new WayBillDetAddProps(),
                        BasePrice = pld.Price,
                        Price = DiscountPrice * 100 / (100 + wb.Nds),
                        WId = _db.Materials.Find(pld.MatId).WId,
                        MatId = pld.MatId.Value
                    });
                }
                _db.SaveChanges();
            }
        }
    }
}