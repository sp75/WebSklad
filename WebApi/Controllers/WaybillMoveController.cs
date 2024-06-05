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
using WebApi.Controllers.Models;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/waybill-move")]
    [ApiTokenAuthorize]
    public class WaybillMoveController : BaseApiController
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
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);

                var wb = sp_base.v_WaybillMove.Where(w => w.OnDate >= req.start_date && w.OnDate < req.end_date && (w.FromWId == ka.WId || w.ToWId == ka.WId)).Select(s => new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.FromWh,
                    s.ToWh,
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

        [HttpGet, Route("create/{dest_wid}")]
        public bool Create(int dest_wid)
        {
            bool result_exe = true;

            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));
                var new_inventory_wb = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = 4,
                    OnDate = DateTime.Now,
                    Num = sp_base.GetDocNum("wb_move").FirstOrDefault(),
                    CurrId = 2,
                    OnValue = 1,
                    WaybillMove = new WaybillMove { SourceWid = ka.WId.Value, DestWId = dest_wid },
                    EntId = _enterprise?.KaId,
                    Checked = 0,
                    Nds = 0
                });

                sp_base.SaveChanges();

                return result_exe;
            }
        }


        [HttpGet, Route("delete/{wbill_id}")]
        public bool DeleteWb(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.WaybillList.Find(wbill_id);
                if (wb.Checked == 0)
                {
                    sp_base.WaybillList.Remove(wb);
                    sp_base.SaveChanges();
                }else
                {
                    return false;
                }

                return true;
            }
        }

        [HttpPost, Route("item/add")]
        public bool AddItem(AddItemRequest req)
        {
            bool result_exe = true;

            using (var sp_base = SPDatabase.SPBase())
            {
                var _wb = sp_base.WaybillList.Find(req.WbillId);
                var mat = sp_base.Materials.FirstOrDefault(w => w.OpenStoreId == req.ArtId);
                if (mat != null)
                {
                    var pos_in = new WaybillMoveRepository().GetPosIn(Context.Token.Value, mat.MatId);

                    if (pos_in.Sum(s => s.CurRemain) >= req.Amount)
                    {
                        int num = 0;
                        var sum_amount = req.Amount;
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
                                var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                                {
                                    WbillId = _wb.WbillId,
                                    Price = item.Price * item.OnValue,
                                    BasePrice = item.BasePrice * item.OnValue,
                                    Nds = _wb.Nds,
                                    CurrId = _wb.CurrId,
                                    OnDate = _wb.OnDate,
                                    WId = item.WId,
                                    Num = ++num,
                                    Amount = item.Amount,
                                    MatId = item.MatId,
                                    OnValue = _wb.OnValue

                                });

                                wbd.WMatTurn1.Add(new WMatTurn
                                {
                                    PosId = item.PosId,
                                    WId = item.WId,
                                    MatId = item.MatId,
                                    OnDate = _wb.OnDate,
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
                }
                else
                {
                    result_exe = false;
                }
            }

            return result_exe;
        }


        [HttpGet, Route("item/{pos_id}/delete")]
        public bool DeleteWbItem(int pos_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                sp_base.DeleteWhere<WaybillDet>(w => w.PosId == pos_id);

                return true;
            }
        }

        [HttpGet, Route("{wbill_id}/execute")]
        public IHttpActionResult ExecuteWbMove(int wbill_id)
        {
            var wb = db.WaybillList.Find(wbill_id);

            if (wb.Checked == 0)
            {
                var ex_wb_move = SPDatabase.SPBase().ExecuteWayBill(wbill_id, 4, null).ToList().FirstOrDefault();

                if (ex_wb_move.ErrorMessage != "False")
                {
                    return Ok(false);
                }
            }

            return Ok(true);
        }

        [HttpGet, Route("{wbill_id}")]
        public IHttpActionResult GetWaybill(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WaybillMove.Where(w=> w.WbillId == wbill_id).Select(s=> new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.FromWh,
                    s.ToWh,
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
