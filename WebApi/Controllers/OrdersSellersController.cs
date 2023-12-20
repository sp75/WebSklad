using SP.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/customer")]
    public class OrdersSellersController : BaseApiController
    {
        [ApiTokenAuthorize]
        [HttpGet, Route("info")]
        public IHttpActionResult GetCustomerInfo()
        {
            using (var sp_base = Database.SPBase())
            {
                return Ok(sp_base.KagentList.FirstOrDefault(w => w.Id == Context.Token));
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("current-orders")]
        public IHttpActionResult GetCurrentOrders()
        {
            var from_dt = DateTime.Now.Date;
            var to_dt = from_dt.AddDays(1);
            var prev_dt = from_dt.AddDays(-7);
            using (var sp_base = Database.SPBase())
            {
                return Ok(sp_base.v_WaybillDet.Where(w => w.WType == -16 && w.KagentId == Context.Token && w.WbOnDate >= from_dt && w.WbOnDate < to_dt && w.WbChecked == 0).OrderBy(o => o.Num).Select(s => new
                {
                    s.PosId,
                    s.Num,
                    s.MatId,
                    s.Artikul,
                    s.Amount,
                    s.MatName,
                    s.WbillId,
                    s.WbNum,
                    s.MsrName,
                    s.Price,
                    s.Total,
                    s.Checked,
                    s.WbOnDate,
                    s.Notes,
                    s.WbNotes,
                    PrevAmount = sp_base.v_WaybillDet.Where(ww=> ww.KagentId == s.KagentId && ww.MatId == s.MatId  && ww.WbChecked == 1 && ww.WType == -16 && ww.WbOnDate>= prev_dt).OrderByDescending(or=> or.OnDate).Select(s3=> s3.Amount).FirstOrDefault()
                }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("current-orders")]
        public IHttpActionResult SetWaybillDet(SetPosAmountRequest In)
        {
            using (var sp_base = Database.SPBase())
            {
                var pos = sp_base.WaybillDet.FirstOrDefault(w => w.PosId == In.PosId);
                if (pos.WaybillList.Checked == 0)
                {
                    pos.Amount = In.Amount;
                    pos.Checked = In.Amount > 0 ? 1 : 0;
                    pos.WaybillList.UpdatedAt = DateTime.Now;
                    //           pos.WaybillList.UpdatedBy = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token)?.KaId;
                    pos.Notes = In.Notes;

                    sp_base.RemoteCustomerOrders.Add(new SP.Base.Models.RemoteCustomerOrders
                    {
                        Amount = In.Amount,
                        CreatedAt = DateTime.Now,
                        MatId = pos.MatId,
                        PosId = In.PosId,
                        WbillId = pos.WbillId,
                        CustomerId = Context.Token.Value
                    });

                    sp_base.SaveChanges();
                }
                else
                {
                    return BadRequest("Замовлення вже закрито !"); // ResponseMessage(Request.CreateResponse(HttpStatusCode.NotModified, new ErrorMessage { message = "Замовлення вже закрито !" }));
                }

                return Ok(sp_base.v_WaybillDet.Where(w => w.PosId == In.PosId).Select(s => new
                {
                    s.PosId,
                    s.MatId,
                    s.Artikul,
                    s.Amount,
                    s.MatName,
                    s.WbillId,
                    s.WbNum,
                    s.MsrName,
                    s.Price,
                    s.Total,
                    s.Checked,
                    s.Notes
                }).FirstOrDefault());
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("last-orders")]
        public IHttpActionResult GetLastService()
        {
            var from_dt = DateTime.Now.Date.AddDays(-30);

            using (var sp_base = Database.SPBase())
            {
                return Ok(sp_base.v_WaybillDet.Where(w => w.WType == -16 && w.KagentId == Context.Token && w.WbOnDate >= from_dt && w.WbChecked == 1).OrderBy(o => o.Num)
                    .Select(s => new
                    {
                        s.PosId,
                        s.Num,
                        s.MatId,
                        s.Artikul,
                        s.Amount,
                        s.MatName,
                        s.WbillId,
                        s.WbNum,
                        s.MsrName,
                        s.Price,
                        s.Total,
                        s.Checked,
                        s.WbOnDate,
                        s.Notes
                    }).ToList());
            }
        }

        public class ErrorMessage
        {
            public string message { get; set; }
        }

        public class SetPosAmountRequest
        {
            public int PosId { get; set; }
            public decimal Amount { get; set; }
            public string Notes { get; set; }
        }
        //Get customer info

    }
}