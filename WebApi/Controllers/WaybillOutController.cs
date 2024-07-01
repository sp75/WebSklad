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
using WebApi.Controllers.Models;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/waybill-out")]
    [ApiTokenAuthorize]
    public class WaybillOutController : BaseApiController
    {

        public class FilterWb
        {
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
        }

        [HttpPost, Route("list")]
        public IHttpActionResult GetWaybill(FilterWb req)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WayBillBase.Where(w => w.OnDate >= req.start_date && w.OnDate < req.end_date && w.KagentId == Context.Token).Select(s => new
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
                    s.WType,
                    OnWh = db.GetRelDocIds(s.Id).Any(a => a.DocType == 1 && a.RelType == 1)
                }).ToList();

                return Ok(wb);
            }
        }


        [HttpGet, Route("{wbill_id}")]
        public IHttpActionResult GetWaybill(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WayBillBase.Where(w=> w.WbillId == wbill_id && w.KagentId == Context.Token).Select(s=> new
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
                    Details = sp_base.v_WayBillOutDet.Where(w => w.WbillId == s.WbillId).Select(s1 => new
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
                        s1.GroupName,
                        s1.Notes,
                        s1.WhName,
                        s1.Total
                    })
                }).FirstOrDefault();

                return Ok(wb);
            }
        }

    }
}
