using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerPayments;
using WebApi.Api.CustomerWayBillIn;
using WebApi.Controllers.Models;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/waybill-in")]
    [ApiTokenAuthorize]
    public class WaybillInController : BaseApiController
    {
        [HttpPost, Route("list")]
        public IHttpActionResult GetWaybill(FilterWb req)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WayBillIn.Where(w => w.OnDate >= req.start_date && w.OnDate < req.end_date && w.ToWId == ka.WId && w.WType == 1).Select(s => new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.KaName,
                    s.SummInCurr,
                    s.ToWh,
                    s.EntName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    s.WType
                }).ToList();

                return Ok(wb);
            }
        }

        [HttpGet, Route("{wbill_id}")]
        public IHttpActionResult GetWaybill(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WayBillIn.Where(w=> w.WbillId == wbill_id).Select(s=> new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.KaName,
                    s.EntName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    Details = sp_base.v_WayBillInDet.Where(w => w.WbillId == s.WbillId).Select(s1 => new
                    {
                        s1.PosId,
                        s1.OnDate,
                        s1.MatName,
                        s1.MatId,
                        s1.MsrName,
                        s1.Amount,
                        s1.BasePrice,
                        s1.Price,
                        s1.Discount,
                        s1.Artikul,
                        s1.GrpName,
                        s1.Notes,
                        s1.WhName,
                        s1.Wid
                    })
                }).FirstOrDefault();

                return Ok(wb);
            }
        }


        [HttpGet, Route("{wbill_id}/det")]
        public IHttpActionResult GetWaybillDet(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var det = sp_base.v_WayBillInDet.Where(w => w.WbillId == wbill_id).Select(s1 => new
                {
                    s1.PosId,
                    s1.Num,
                    s1.OnDate,
                    s1.MatName,
                    s1.MatId,
                    s1.MsrName,
                    s1.Amount,
                    s1.BasePrice,
                    s1.Price,
                    s1.Discount,
                    s1.Artikul,
                    s1.Notes,
                    s1.WhName,
                    s1.Total
                }).ToList();

                return Ok(det);
            }
        }

        [HttpPost, Route("create")]
        public bool Create(CreateWayBillInRequest req)
        {
            bool result_exe = true;

            using (var sp_base = SPDatabase.SPBase())
            {
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));
                var wb_in = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = 1,
                    OnDate = req.OnDate,
                    Num = req.Num,
                    CurrId = 2,
                    OnValue = 1,
                    WaybillMove = new WaybillMove { SourceWid = ka.WId.Value,  DestWId = ka.WId.Value },
                    EntId = _enterprise?.KaId,
                    Checked = 0,
                    Nds = 0,
                    Notes = req.Notes,
                    PersonId = ka.KaId,
                    UpdatedAt = DateTime.Now,
                    KaId = req.KaId
                });
                sp_base.SaveChanges();

                foreach (var item in req.Items)
                {
                    var mat = sp_base.Materials.FirstOrDefault(w => w.MatId == item.MatId);
                    if (mat != null)
                    {
                        var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = wb_in.WbillId,
                            Num = wb_in.WaybillDet.Count() + 1,
                            Amount = item.Amount,
                            OnValue = wb_in.OnValue,
                            WId = ka.WId,
                            Nds = wb_in.Nds,
                            CurrId = wb_in.CurrId,
                            OnDate = wb_in.OnDate,
                            MatId = item.MatId,
                            Price = item.Price,
                            BasePrice = item.Price
                        });
                    }
                }
                sp_base.SaveChanges();
              

                foreach (var det_item in sp_base.WaybillDet.Where(w => w.WbillId == wb_in.WbillId).ToList())
                {
                    sp_base.WMatTurn.Add(new WMatTurn
                    {
                        PosId = det_item.PosId,
                        WId = det_item.WId.Value,
                        MatId = det_item.MatId,
                        OnDate = det_item.OnDate.Value,
                        TurnType = 1,
                        Amount = det_item.Amount,
                        SourceId = det_item.PosId
                    });
                }

                wb_in.Checked = 1;

                sp_base.SaveChanges();
                sp_base.Entry(wb_in).Reload();

                if (req.CreatePayment && wb_in.SummInCurr.HasValue)
                {
                    var rep = new CustomerPaymentsRepository();

                    var new_pd = rep.NewPayDocOut(Context.Token, wb_in.SummInCurr.Value, 1, req.KaId, req.Notes);
                    if (new_pd != null)
                    {
                        sp_base.DocRels.Add(new DocRels { OriginatorId = wb_in.Id, RelOriginatorId = new_pd.Id });
                        sp_base.SaveChanges();
                    }

                }

                return result_exe;
            }
        }
    }
}
