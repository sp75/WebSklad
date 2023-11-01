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
                    s.Notes
                }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("current-orders")]
        public IHttpActionResult SetAmount(SetPosAmountRequest In)
        {
            using (var sp_base = Database.SPBase())
            {
                var pos = sp_base.WaybillDet.FirstOrDefault(w => w.PosId == In.PosId);
                if (pos.WaybillList.Checked == 0)
                {
                    pos.Amount = In.Amount;
                    pos.Checked = 1;
                    pos.WaybillList.UpdatedAt = DateTime.Now;

                    sp_base.SaveChanges();
                }
                else
                {
                    return Ok(new ErrorMessage { message = "Замовлення вже закрито !" });
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
                    s.Checked
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
        }
        //Get customer info

    }
}