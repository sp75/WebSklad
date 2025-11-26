using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerPayments;
using WebApi.Api.CustomerReturns;
using WebApi.Api.CustomerSales;
using WebApi.Api.OpenStore;
using WebApi.Controllers.Models;
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
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.KagentList.FirstOrDefault(w => w.Id == Context.Token));
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("current-sales")]
        public IHttpActionResult GetCurrentSales()
        {
            var ka_sales_out = new CustomerSalesRepository().GetCurrentSales(Context.Token.Value);
            return Ok(ka_sales_out);
        }


        [ApiTokenAuthorize]
        [HttpGet, Route("current-orders")]
        public IHttpActionResult GetCurrentOrders()
        {
            var from_dt = DateTime.Now.Date;
            var to_dt = from_dt.AddDays(1);
            var prev_dt = from_dt.AddDays(-7);
            using (var sp_base = SPDatabase.SPBase())
            {
                /*  return Ok(sp_base.v_WaybillDet.Where(w => w.WType == -16 && w.KagentId == Context.Token && w.WbOnDate >= from_dt && w.WbOnDate < to_dt && w.WbChecked == 0).OrderBy(o => o.Num).Select(s => new
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
                      PrevAmount = sp_base.v_WaybillDet.Where(ww => ww.KagentId == s.KagentId && ww.MatId == s.MatId && ww.WbChecked == 1 && ww.WType == -16 && ww.WbOnDate >= prev_dt).OrderByDescending(or => or.OnDate).Select(s3 => s3.Amount).FirstOrDefault()
                  }).ToList());*/

                return Ok(sp_base.v_wrd_CustomerOrders.Where(w => w.KagentId == Context.Token && w.WbOnDate >= from_dt/* && w.WbOnDate < to_dt */).OrderBy(o => o.Num).Select(s => new
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
                    s.PrevAmount,
                    s.ToDate
                }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("current-orders")]
        public IHttpActionResult SetWaybillDet(SetPosAmountRequest In)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var pos = sp_base.WaybillDet.FirstOrDefault(w => w.PosId == In.PosId);
                if (pos.WaybillList.Checked == 0 && (!pos.WaybillList.ToDate.HasValue || pos.WaybillList.ToDate > DateTime.Now))
                {
                    var msr = sp_base.MaterialMeasures.Where(w => w.MatId == pos.MatId && w.UseInOrders == true).FirstOrDefault();

                    var pos_date = DateTime.Now;

                    pos.Amount = msr != null && msr.Amount > 0 ? In.Amount / msr.Amount : In.Amount;
                    pos.Checked = In.Amount > 0 ? 1 : 0;
                    pos.UpdateAt = pos_date;
                    pos.Notes = In.Notes;

                    pos.WaybillList.UpdatedAt = pos_date;
                    sp_base.SaveChanges();

                    sp_base.RemoteCustomerOrders.Add(new SP.Base.Models.RemoteCustomerOrders
                    {
                        Amount = msr != null && msr.Amount > 0 ? In.Amount / msr.Amount : In.Amount,
                        CreatedAt = pos_date,
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

                return Ok(sp_base.v_wrd_CustomerOrders.Where(w => w.PosId == In.PosId).Select(s => new
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

            using (var sp_base = SPDatabase.SPBase())
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
                        s.Notes,
                        s.WbNotes
                    }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("last-orders-v2")]
        public IHttpActionResult GetLastServiceV2()
        {
            var from_dt = DateTime.Now.Date.AddDays(-30);

            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_WayBillBase.Where(w => w.WType == -16 && w.KagentId == Context.Token && w.OnDate >= from_dt && w.Checked == 1).OrderByDescending(o => o.OnDate)
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
        [HttpGet, Route("{WbillId}/last-orders-daetails")]
        public IHttpActionResult GetLasOrdersDet(int WbillId)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_WayBillCustomerOrderDet.Where(w => w.WbillId == WbillId).Join(sp_base.v_WayBillBase.Where(w => w.KagentId == Context.Token), det => det.WbillId,
                                    wb => wb.WbillId,
                                   (det, wb) => det)
                    .Select(s => new
                    {
                        s.PosId,
                        s.Num,
                        s.MatName,
                        s.Artikul,
                        s.Amount,
                        s.MsrName,
                        s.Price
                    }).OrderBy(o => o.Num).ToList());
            }
        }




        [ApiTokenAuthorize]
        [HttpGet, Route("returned-invoices")]
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
        [HttpGet, Route("{WbillId}/returned-invoices-daetails")]
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
        [HttpGet, Route("current-returned")]
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
                        OutPosPrice = s.WaybillDet_OutPosId != null ? s.WaybillDet_OutPosId.BasePrice : sp_base.PosRemains.Where(w2 => w2.MatId == s.MatId && w2.Remain > 0).OrderByDescending(o2 => o2.OnDate).Select(sr=> sr.AvgPrice).FirstOrDefault(),
                        OutPosOnDate = s.WaybillDet_OutPosId.OnDate
                    }).ToList());
            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("current-returned")]
        public IHttpActionResult SetCustomerReturned(CustomerReturnedRequest In)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var new_item = sp_base.RemoteCustomerReturned.Add(new SP.Base.Models.RemoteCustomerReturned
                {
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

        [ApiTokenAuthorize]
        [HttpPost, Route("current-returned/{id}/del")]
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
        [HttpPost, Route("create-returned")]
        public IHttpActionResult CreateReturned()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token.Value);

                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));

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
                    EntId = _enterprise?.KaId,
                    ReportingDate = DateTime.Now,
                    PTypeId = 1
                });
                sp_base.SaveChanges();


                foreach (var pos_out in new CustomerReturnsRepository().GetReturnetPosOut(Context.Token.Value))
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
                }
                else
                {
                    sp_base.WaybillList.Remove(sp_base.WaybillList.Find(_wb.WbillId));
                }

                sp_base.SaveChanges();
            }

            return Ok(true);
        }


        [ApiTokenAuthorize]
        [HttpGet, Route("{mat_id}/pos-out")]
        public IHttpActionResult GetMatPosOut(int mat_id)
        {
            DateTime start_date = DateTime.Now.Date.AddDays(-30);

            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.Database.SqlQuery<CustomerPosOut>(@"
  select wbd.PosId, 
         wbl.WbillId, 
		 wbl.Num, 
		 wbl.OnDate, 
         wbd.Amount, 
		 wbd.Price, 
		 r.ReturnAmount,
     	 (wbd.Amount - coalesce( r.ReturnAmount, 0) - coalesce( rcr.RemotePosAmount, 0) ) Remain
  from WaybillDet wbd
  join WaybillList wbl on wbl.wbillid=wbd.wbillid
  join Materials m on m.matid=wbd.matid
  join Kagent ka on ka.kaid=wbl.kaid
  left join (select rr.OutPosId, 
	               sum(wbd_r.amount) ReturnAmount 
	         from ReturnRel rr, WaybillDet wbd_r 
		     where  wbd_r.PosId = rr.PosId 
			 group by  rr.OutPosId) r on r.OutPosId =wbd.PosId
  left join (select OutPosId, sum(amount) RemotePosAmount from RemoteCustomerReturned group by OutPosId) rcr on rcr.OutPosId = wbd.PosId
  where  wbl.OnDate between {2} and GETDATE()
         and m.MatId = {0}
         and ka.Id = {1}
         and wbl.Checked = 1
	     and wbl.WType = -1", mat_id, Context.Token, start_date).ToList());
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

        public class CustomerReturnedRequest
        {
            public int MatId { get; set; }
            public int? OutPosId { get; set; }
            public decimal Amount { get; set; }
            public int? InPosId { get; set; }
        }

    }
}