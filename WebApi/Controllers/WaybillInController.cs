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
                    s.WType,
                    s.SummPay
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
                    s.KaId,
                    s.KaName,
                    s.EntName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    s.ToWh,
                    Details = sp_base.v_WayBillInDet.Where(w => w.WbillId == s.WbillId).Select(s1 => new
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
                        s1.GrpName,
                        s1.Notes,
                        s1.WhName,
                        s1.Wid,
                        s1.Total 
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
                /* var det = sp_base.v_WayBillInDet.Where(w => w.WbillId == wbill_id).Select(s1 => new
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
                     s1.Total,
                 }).ToList();*/
                var det = (from s1 in sp_base.v_WayBillInDet
                           join m in sp_base.Materials on s1.MatId equals m.MatId
                           join w in sp_base.v_WayBillIn on s1.WbillId equals w.WbillId
                           join ka in sp_base.Kagent on w.ToWId equals ka.WId
                           join p in sp_base.v_KagentMaterilPrices
                                on new { MatId = (int)s1.MatId, KaId = (int)ka.KaId }
                                equals new { MatId = (int)p.MatId, KaId = (int)p.KaId } into prices
                           from p in prices.DefaultIfEmpty()
                           where s1.WbillId == wbill_id
                           select new
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
                               s1.Total,
                               m.LabelDescr,
                               SalePrice = (decimal?)p.Price // Приведення до nullable, бо це Left Join
                           }).ToList();

                return Ok(det);
            }
        }

        [HttpPost, Route("save")]
        public IHttpActionResult SaveWaybill(CreateWayBillInRequest req)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                // Відкриваємо транзакцію
                using (var dbTransaction = sp_base.Database.BeginTransaction())
                {
                    try
                    {
                        WaybillList wb_in = null;

                        if (!req.WbillId.HasValue)
                        {
                            // Логіка створення нової накладної
                            var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));

                            wb_in = new WaybillList()
                            {
                                Id = Guid.NewGuid(),
                                WType = 1,
                                OnDate = req.OnDate,
                                Num = req.Num,
                                CurrId = 2,
                                OnValue = 1,
                                WaybillMove = new WaybillMove { SourceWid = ka.WId.Value, DestWId = ka.WId.Value },
                                EntId = _enterprise?.KaId,
                                Checked = 0,
                                Notes = req.Notes,
                                PersonId = ka.KaId,
                                UpdatedAt = DateTime.Now,
                                KaId = req.KaId,
                                PTypeId = 1
                            };
                            sp_base.WaybillList.Add(wb_in);
                            sp_base.SaveChanges(); // Отримуємо WbillId
                        }
                        else
                        {
                            // Логіка редагування: видаляємо старі позиції
                            wb_in = sp_base.WaybillList.Find(req.WbillId.Value);
                            if (wb_in == null) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Накладну не знайдено"));


                            var oldItems = sp_base.WaybillDet.Where(w => w.WbillId == req.WbillId.Value);
                            sp_base.WaybillDet.RemoveRange(oldItems);
                            sp_base.SaveChanges();

                            // Оновлюємо шапку
                            wb_in.OnDate = req.OnDate;
                            wb_in.Num = req.Num;
                            wb_in.Notes = req.Notes;
                            wb_in.KaId = req.KaId;
                        }

                        // Додаємо нові позиції
                        int positionCounter = 1;
                        foreach (var item in req.Items)
                        {
                            sp_base.WaybillDet.Add(new WaybillDet()
                            {
                                WbillId = wb_in.WbillId,
                                Num = positionCounter++,
                                Amount = item.Amount,
                                OnValue = wb_in.OnValue,
                                WId = ka.WId,
                                MatId = item.MatId,
                                Price = item.Price,
                                BasePrice = item.Price,
                                OnDate = wb_in.OnDate,
                                CurrId = wb_in.CurrId
                            });
                        }
                        sp_base.SaveChanges();
                        wb_in.UpdatedAt = DateTime.Now;
                        sp_base.SaveChanges();

                        // Якщо все пройшло успішно — фіксуємо зміни в БД
                        dbTransaction.Commit();
                        return Ok(wb_in.WbillId);
                    }
                    catch (Exception ex)
                    {
                        // При будь-якій помилці транзакція скасується автоматично при Dispose
                        dbTransaction.Rollback();
                        return Content(HttpStatusCode.InternalServerError, $"Помилка: {ex.Message}");
                    }
                }
            }
        }


        [HttpGet, Route("{wbill_id}/execute")]
        public bool ExecuteWaybill(int wbill_id, bool pay_document=false)
        {
            bool result_exe = true;

            using (var sp_base = SPDatabase.SPBase())
            {
                var wb_in = sp_base.WaybillList.Find(wbill_id);

                foreach (var det_item in sp_base.WaybillDet.Where(w => w.WbillId == wbill_id).ToList())
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

                if (pay_document && wb_in.SummInCurr.HasValue)
                {
                    var rep = new CustomerPaymentsRepository();

                    var new_pd = rep.NewPayDocOut(Context.Token, wb_in.SummInCurr.Value, 1, wb_in.KaId, wb_in.Notes);
                    if (new_pd != null)
                    {
                        sp_base.DocRels.Add(new DocRels { OriginatorId = wb_in.Id, RelOriginatorId = new_pd.Id });
                        sp_base.SaveChanges();
                    }
                }

                sp_base.RecalcKaSaldo(wb_in.KaId.Value);

                return result_exe;
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
                    KaId = req.KaId,
                    PTypeId = 1
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
                sp_base.RecalcKaSaldo(req.KaId);

                return result_exe;
            }
        }
    }
}
