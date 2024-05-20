using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Controllers.Models;
using WebApi.Core;
using static WebApi.Controllers.OrdersSellersController;

namespace WebApi.Controllers
{
    [RoutePrefix("api/customer-return")]
    [ApiTokenAuthorize]
    public class ReturnSupplierController : BaseApiController
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
                        OutPosPrice = s.WaybillDet_OutPosId != null ? s.WaybillDet_OutPosId.BasePrice : sp_base.v_MatRemains.Where(w2 => w2.MatId == s.MatId).OrderByDescending(o2 => o2.OnDate).FirstOrDefault().AvgPrice,
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
                    OutPosId = In.OutPosId,
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

        [HttpGet, Route("{mat_id}/pos-in")]
        public IHttpActionResult GetMatPosIn(int mat_id)
        {
            DateTime start_date = DateTime.Now.Date.AddDays(-30);

            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.Database.SqlQuery<CustomerPosIn>(@"
select item.* 
from
(
    select pr.PosId, (pr.remain-pr.rsv) as CurRemain, pr.Rsv, wbd.OnDate, wbd.Price, 
           wbl.num as DocNum, wbl.OnDate as DocDate, 
           wbl.WType, wbl.WbillId, wbd.BasePrice, wbl.KaId, pr.Remain,  pr.MatId, pr.WId, wbd.PosParent
    from posremains pr
         left outer join serials s on s.posid=pr.posid
         join waybilldet wbd on wbd.posid=pr.posid
         join waybilllist wbl on wbl.wbillid=wbd.wbillid
         join materials mats on mats.matid=pr.matid
         join measures msr on msr.mid=mats.mid
    where pr.ondate=(select max(ondate)
                     from posremains
                     where posid=pr.posid )
          and pr.matid = {0}
          and wbl.wtype not in(4,6,25)
          and pr.remain > 0 

    union all

    select pr.posid, (pr.remain-pr.rsv) as CURREMAIN, pr.rsv, wbl.ondate,
           wbd.price,  wbl.num as docnum, wbl.ondate as docdate, 
           wbl.wtype, wbl.wbillid, wbd.baseprice, wblext.KaId, pr.Remain, pr.matid, pr.wid, wbd.PosParent
    from posremains pr
         join waybilldet wbd on wbd.posid=pr.posid
         join waybilllist wbl on wbl.wbillid=wbd.wbillid
         join materials mats on mats.matid=pr.matid
         join measures msr on msr.mid=mats.mid
         left outer join extrel er on er.intposid=pr.posid
         left outer join waybilldet wbdext on wbdext.posid=er.extposid
         left outer join waybilllist wblext on wblext.wbillid=wbdext.wbillid
    where pr.ondate=(select max(ondate)
                     from posremains
                     where posid=pr.posid)
          and pr.matid = {0}
          and wbl.wtype in (4,6,25)
          and pr.remain > 0
) item 
inner join Kagent k on k.WId = item.WId
where k.id = {1} and item.DocDate > {2}", mat_id, Context.Token, start_date).ToList());
            }
        }
    }
}