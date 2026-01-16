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
using WebApi.Api.CustomerMove;
using WebApi.Api.CustomerWriteOf;
using WebApi.Controllers.Models;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/waybill-write-of")]
    [ApiTokenAuthorize]
    public class WaybillWriteOfController : BaseApiController
    {
        [HttpPost, Route("list")]
        public IHttpActionResult GetWaybill(FilterWb req)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
               // var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);

                var wb = sp_base.v_WaybillWriteOff.Where(w => w.OnDate >= req.start_date && w.OnDate < req.end_date && w.FromWId == ka.WId ).Select(s => new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.FromWh,
                    s.EntName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    s.WType,
                    IsExecuteDocument = (s.FromWId != ka.WId)
                }).ToList();

                return Ok(wb);
            }
        }

        [HttpGet, Route("{wbill_id}/det")]
        public IHttpActionResult GetWaybillDet(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var det = sp_base.v_WayBillOutDet.Where(w => w.WbillId == wbill_id).Select(s1 => new
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
                }).ToList();
               
                return Ok(det);
            }
        }

        [HttpPost, Route("create")]
        public bool Create(CreateDocumentRequest req)
        {
            bool result_exe = true;

            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));
                var new_wb_write_off = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = -5,
                    OnDate = req.OnDate,
                    Num = req.Num,//sp_base.GetDocNum("wb_write_off").FirstOrDefault(),
                    CurrId = 2,
                    OnValue = 1,
                    WaybillMove = new WaybillMove { SourceWid = ka.WId.Value },
                    EntId = _enterprise?.KaId,
                    Checked = 0,
                    Nds = 0,
                    Notes = req.Notes,
                    PersonId = ka.KaId,
                    UpdatedAt = DateTime.Now
                });
                sp_base.SaveChanges();

                foreach(var wb_item in req.Items)
                {
                    var mat = sp_base.Materials.FirstOrDefault(w => w.MatId == wb_item.MatId);
                    if (mat != null)
                    {
                        var pos_in = new WaybillMoveRepository().GetPosIn(Context.Token.Value, mat.MatId);

                        var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = new_wb_write_off.WbillId,
                            Price = 0,
                            BasePrice = 0,
                            Nds = new_wb_write_off.Nds,
                            CurrId = new_wb_write_off.CurrId,
                            OnDate = new_wb_write_off.OnDate,
                            WId = ka.WId.Value,
                            Num = wb_item.Num,
                            Amount = wb_item.Amount,
                            MatId = mat.MatId,
                            OnValue = 1

                        });

                        if (pos_in.Sum(s => s.CurRemain) >= wb_item.Amount)
                        {
                            var sum_amount = wb_item.Amount;
                            bool stop = false;
                            foreach (var item in pos_in)
                            {
                                if (!stop)
                                {
                                    if (sum_amount <= item.CurRemain)
                                    {
                                        item.Amount = sum_amount;
                                        sum_amount -= item.CurRemain;
                                        stop = true;
                                    }
                                    else
                                    {
                                        item.Amount = item.CurRemain;
                                        sum_amount -= item.CurRemain;
                                    }
                                }

                                if (item.Amount > 0)
                                {
                                     wbd.WMatTurn1.Add(new WMatTurn
                                    {
                                        PosId = item.PosId,
                                        WId = item.WId,
                                        MatId = item.MatId,
                                        OnDate = new_wb_write_off.OnDate,
                                        TurnType = 2,
                                        Amount = Convert.ToDecimal(item.Amount),
                                        //  SourceId = wbd.PosId
                                    });
                                }
                            }
                            sp_base.SaveChanges();
                        }
                        else
                        {
                            result_exe = false;
                        }

                        decimal? selamount = pos_in.Sum(s => s.Amount);
                        decimal? sum = pos_in.Sum(s => s.Amount * s.Price * s.OnValue);

                        if (selamount > 0)
                        {
                            wbd.Price = sum / selamount;
                            wbd.BasePrice = wbd.Price * wbd.OnValue;
                        }

                        sp_base.SaveChanges();
                    }
                    else
                    {
                        result_exe = false;
                    }
                }

                if (new_wb_write_off.Checked == 0)
                {
                    var ex_wb_move = SPDatabase.SPBase().ExecuteWayBill(new_wb_write_off.WbillId, null, null).ToList().FirstOrDefault();

                    if (ex_wb_move.ErrorMessage != "False")
                    {
                        result_exe = false;
                    }
                }

                return result_exe;
            }
        }
    }
}
