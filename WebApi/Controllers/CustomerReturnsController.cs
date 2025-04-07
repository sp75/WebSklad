using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerReturns;
using WebApi.Controllers.Models;
using WebApi.Core;
using static WebApi.Controllers.OrdersSellersController;

namespace WebApi.Controllers
{
    [RoutePrefix("api/customer-return")]
    [ApiTokenAuthorize]
    public class CustomerReturnsController : BaseApiController
    {


        [ApiTokenAuthorize]
        [HttpGet, Route("waybills")]
        public IHttpActionResult GetReturnedInvoices()
        {
            var from_dt = DateTime.Now.Date.AddDays(-30);
            var to_dt = DateTime.Now.Date.AddDays(1);
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_WayBillBase.Where(w => w.WType == 6 && w.KagentId == Context.Token && w.OnDate >= from_dt && w.OnDate < to_dt)
                    .OrderByDescending(o => o.OnDate)
                    .Select(s => new
                    {
                        s.WbillId,
                        s.Checked,
                        s.Num,
                        s.OnDate,
                        s.SummAll,
                        s.Notes,
                        s.Reason,
                        s.PersonName
                    }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("{WbillId}/waybill-details")]
        public IHttpActionResult GetReturnedInvoicesDet(int WbillId)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_WayBillReturnCustomerDet.Where(w => w.WbillId == WbillId).Join(sp_base.v_WayBillBase.Where(w => w.KagentId == Context.Token), det => det.WbillId,
                                    wb => wb.WbillId,
                                   (det, wb) => det)
                    .Select(s => new
                    {
                        s.PosId,
                        s.Num,
                        s.MatName,
                        s.Artikul,
                        s.Amount,
                        s.MsrName
                    }).OrderBy(o => o.Num).ToList());
            }
        }


        [ApiTokenAuthorize]
        [HttpGet, Route("current")]
        public IHttpActionResult GetCurrentReturned()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.RemoteCustomerReturned.Where(w => w.CustomerId == Context.Token && (w.WbillId == null || w.WaybillList.Checked == 0))
                    .OrderBy(o => o.CreatedAt)
                    .Select(s => new
                    {
                        s.Id,
                        MatName = s.Materials.Name,
                        Artikul = s.Materials.Artikul,
                        s.Amount,
                        MsrName = s.Materials.Measures.ShortName,
                        s.CreatedAt,
                        s.WbillId,
                        WbNum = s.WaybillList.Num,
                        WbOnDate = (DateTime?)s.WaybillList.OnDate,
                        OutPosAmount = s.WaybillDet_OutPosId != null ? s.WaybillDet_OutPosId.Amount : (decimal?)null,
                        OutPosPrice = s.WaybillDet_OutPosId != null ? s.WaybillDet_OutPosId.BasePrice : sp_base.PosRemains.Where(w2 => w2.MatId == s.MatId && w2.Remain > 0).OrderByDescending(o2 => o2.OnDate).Select(sr => sr.AvgPrice).FirstOrDefault(),
                        OutPosOnDate = s.WaybillDet_OutPosId.OnDate
                    }).ToList());
            }
        }

        [HttpPost, Route("current/add")]
        public IHttpActionResult SetCustomerReturned(CustomerReturnedRequest In)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var new_item = sp_base.RemoteCustomerReturned.Add(new SP.Base.Models.RemoteCustomerReturned
                {
                    PosId = In.InPosId,
                    Amount = In.Amount,
                    CreatedAt = DateTime.Now,
                    CustomerId = Context.Token.Value,
                    MatId = In.MatId,
                    OutPosId = In.OutPosId == 0 ? null : In.OutPosId,
                });

                sp_base.SaveChanges();

                return Ok(new_item);
            }
        }


        [HttpPost, Route("current/{id}/del")]
        public bool DeleteReturned(int id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var item = sp_base.RemoteCustomerReturned.Find(id);
                if (!item.WbillId.HasValue)
                {
                    sp_base.RemoteCustomerReturned.Remove(item);
                    sp_base.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("create-documents")]
        public IHttpActionResult CreateReturned()
        {
            var wb_list = new CustomerReturnsRepository().GreateReturnToSupplier(Context.Token.Value);

            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.v_Kagent.FirstOrDefault(w => w.Id == Context.Token.Value);

                var _wb = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = 6,
                    OnDate = DateTime.Now,
                    Num = sp_base.GetDocNum("wb(6)").FirstOrDefault(),
                    KaId = ka.KaId,
                    CurrId = 2,
                    OnValue = 1,
                    Notes = "віддалене повернення",
                    EntId = ka.EnterpriseId,
                    ReportingDate = DateTime.Now,
                    PTypeId = 1
                });
                sp_base.SaveChanges();


                foreach (var pos_out in new CustomerReturnsRepository().GetReturnetPosOut(Context.Token.Value).Where(w=> w.WbillIdOut != null).ToList())
                {
                    bool stop = false;
                    int num = 1;
                    decimal amount = pos_out.RemoteAmount;

                    foreach (var pos_in in new CustomerReturnsRepository().GetPosIn(pos_out.PosId).Where(w => w.Remain > 0))
                    {
                        if (!stop)
                        {
                            var t_wbd = sp_base.WaybillDet.Add(new WaybillDet
                            {
                                WbillId = _wb.WbillId,
                                Price = pos_out.Price,
                                BasePrice = pos_out.BasePrice,
                                Nds = pos_out.Nds,
                                CurrId = pos_out.CurrId,
                                OnValue = pos_out.OnValue,
                                OnDate = pos_out.OnDate,
                                WId = pos_out.WId,
                                MatId = pos_out.MatId,
                                Discount = pos_out.Discount,
                                Num = ++num
                            });

                            if (pos_in.Remain >= amount)
                            {
                                t_wbd.Amount = amount;
                                stop = true;
                            }
                            else
                            {
                                t_wbd.Amount = pos_in.Remain.Value;

                                amount -= pos_in.Remain.Value;
                            }
                            sp_base.SaveChanges();

                            sp_base.ReturnRel.Add(new ReturnRel
                            {
                                PosId = t_wbd.PosId,
                                OutPosId = pos_out.PosId,
                                PPosId = pos_in.PosId
                            });
                            sp_base.SaveChanges();
                        }
                    }

                    var rcr = sp_base.RemoteCustomerReturned.Find(pos_out.RemoteId);
                    rcr.WbillId = _wb.WbillId;

                    sp_base.SaveChanges();
                }

                int tmc_num = sp_base.WaybillDet.Where(w => w.WbillId == _wb.WbillId).Count();
                foreach (var tmc in sp_base.RemoteCustomerReturned.Where(w => w.CustomerId == Context.Token.Value && w.WbillId == null && w.OutPosId == null).ToList())
                {
                    var _wbt = sp_base.WayBillTmc.Add(new WayBillTmc()
                    {
                        WbillId = _wb.WbillId,
                        Amount = tmc.Amount,
                        TurnType = _wb.WType > 0 ? 1 : -1,
                        Num = ++tmc_num,
                        MatId = tmc.MatId,
                        Price = sp_base.v_MatRemains.Where(w => w.MatId == tmc.MatId).OrderByDescending(o => o.OnDate).FirstOrDefault()?.AvgPrice
                    });

                    //      var rcr = sp_base.RemoteCustomerReturned.Find(tmc.Id);
                    tmc.WbillId = _wb.WbillId;

                    sp_base.SaveChanges();
                }


                if (sp_base.WaybillDet.Any(a => a.WbillId == _wb.WbillId) || sp_base.WayBillTmc.Any(a => a.WbillId == _wb.WbillId))
                {
                    _wb.UpdatedAt = DateTime.Now;

                    foreach (var i in wb_list)
                    {
                        sp_base.DocRels.Add(new DocRels { OriginatorId = i, RelOriginatorId = _wb.Id });
                    }
                }
                else
                {
                    sp_base.WaybillList.Remove(sp_base.WaybillList.Find(_wb.WbillId));
                }

                sp_base.SaveChanges();
            }

            return Ok(true);
        }

        [HttpGet, Route("{mat_id}/pos-remain")]
        public IHttpActionResult GetPosRemain(int mat_id)
        {
            DateTime start_date = DateTime.Now.Date.AddDays(-30);

            using (var sp_base = SPDatabase.SPBase())
            {
                /*        var list =    sp_base.Database.SqlQuery<CustomerPosIn>(@"
           select item.* , (CurRemain - (select coalesce( sum([Amount]), 0 ) from RemoteCustomerReturned where PosId = item.PosId ) ) TotalRemain
           from
           (
               select pr.PosId, (pr.remain-pr.rsv) as CurRemain, pr.Rsv, wbd.OnDate, wbd.Price, 
                      wbl.num as DocNum, wbl.OnDate as DocDate, 
                      wbl.WType, wbl.WbillId, wbd.BasePrice, pr.SupplierId KaId, pr.Remain,  pr.MatId, pr.WId,  (case when coalesce(wbd.PosParent, 0) = 0 then pr.PosId else wbd.PosParent end) PosParent
               from posremains pr
                    left outer join serials s on s.posid=pr.posid
                    join waybilldet wbd on wbd.posid=pr.posid
                    join waybilllist wbl on wbl.wbillid=wbd.wbillid
               where pr.ondate=(select max(ondate)
                                from posremains
                                where posid=pr.posid )
                     and pr.matid = {0}
                     and pr.remain > 0 
              --       and pr.SupplierId is not null
           ) item 
           inner join Kagent k on k.WId = item.WId
           where k.id = {1} and item.DocDate > {2} and item.PosId != item.PosParent", mat_id, Context.Token, start_date).ToList();*/

                var list = sp_base.Database.SqlQuery<CustomerPosIn>(@"
   select wbd.PosId, 
         wbd.PosId PosParent, 
         wbl.WbillId, 
		 wbl.Num DocNum, 
		 wbl.OnDate DocDate, 
         wbd.OnDate,
         wbd.Amount, 
		 wbd.Price, 
         wbd.BasePrice,
		 r.ReturnAmount,
     	 (wbd.Amount - coalesce( r.ReturnAmount, 0) - coalesce( rcr.RemotePosAmount, 0) ) TotalRemain,
		 (wbd.Amount - coalesce( rcr.RemotePosAmount, 0) ) RemoteRemain,
		 m.MatId,
		 rcr.RemotePosAmount
  from WaybillDet wbd
  join WaybillList wbl on wbl.wbillid=wbd.wbillid
  join Materials m on m.matid=wbd.matid
  join Kagent ka on ka.kaid=wbl.kaid
  left join (select rr.OutPosId, 
	               sum(wbd_r.amount) ReturnAmount 
	         from ReturnRel rr, WaybillDet wbd_r 
		     where  wbd_r.PosId = rr.PosId 
			 group by  rr.OutPosId) r on r.OutPosId =wbd.PosId
  left join (select OutPosId, sum(amount) RemotePosAmount from RemoteCustomerReturned where WbillId is null group by OutPosId) rcr on rcr.OutPosId = wbd.PosId
  where  wbl.OnDate > {2}
         and m.MatId = {0}
         and ka.Id = {1}
         and wbl.Checked = 1
	     and wbl.WType = -1", mat_id, Context.Token, start_date).ToList();


                return Ok(list);
            }
        }
    }
}