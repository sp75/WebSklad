using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerInventory;
using WebApi.Api.OpenStore;
using WebApi.Controllers.Models;
using WebApi.Core;


namespace WebApi.Controllers
{
    [RoutePrefix("api/inventory")]
    [ApiTokenAuthorize]
    public class InventoryController :  BaseApiController
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        [HttpPost, Route("checking")]
        public IHttpActionResult CheckingInventoryAct(List<InventoryActDet> req)
        {
            try
            {
                var ka = db.Kagent.FirstOrDefault(w => w.Id == Context.Token);
                var art_list = req.Select(s => s.ARTID).ToList();

                var wh = new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value).Where(w => (w.TypeId == 1 || w.TypeId == 5 || w.TypeId == 6) && w.OpenStoreId != null).ToList();

                var list = wh.Where(w => !art_list.Contains(w.OpenStoreId ?? 0)).Select(s => new
                {
                    s.OpenStoreId,
                    s.CurRemain,
                    s.MatName,
                    s.Artikul
                }).ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                var message = string.Format("| {0} | Error", Context.Token);
                _log.LogException(ex, message);

                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("execute")]
        public bool Execute(List<InventoryActDet> req)
        {
            if (!new OpenStoreRepository().ImportKagentSales(Context.Token))
            {
                return false;
            }

            bool result_exe = true;
            string log_msg="";

            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka.KaId));
                var new_inventory_wb = sp_base.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = 7,
                    OnDate = DateTime.Now,
                    Num = sp_base.GetDocNum("wb_inventory").FirstOrDefault(),
                    CurrId = 2,
                    OnValue = 1,
                    // PersonId = DBHelper.CurrentUser.KaId,
                    WaybillMove = new WaybillMove { SourceWid = ka.WId.Value },
                    //    UpdatedBy = DBHelper.CurrentUser.UserId,
                    EntId = _enterprise?.KaId,
                    Checked = 1,
                    Notes = "Віддалена інвентаризація торгової точки"
                });

                sp_base.SaveChanges();

                var wh = new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value);

                int num = 0;
                foreach (var item in req)
                {
                    var mat = sp_base.Materials.FirstOrDefault(w => w.OpenStoreId == item.ARTID);

                    if (mat != null)
                    {
                        var mat_on_wh = wh.FirstOrDefault(w => w.MatId == mat.MatId);
                        sp_base.WaybillDet.Add(new WaybillDet
                        {
                            WbillId = new_inventory_wb.WbillId,
                            MatId = mat.MatId,
                            WId = new_inventory_wb.WaybillMove.SourceWid,
                            Amount = mat_on_wh != null ? mat_on_wh.CurRemain : 0,
                            Price = item.Price,
                            Discount = item.Amount,
                            Nds = item.Price,
                            OnDate = new_inventory_wb.OnDate,
                            Num = ++num,
                            BasePrice = item.Price,
                            Notes = item.CreatedAt?.ToString()
                        }); 
                    }
                }
                sp_base.SaveChanges();

                if (sp_base.WaybillDet.Where(w => w.WbillId == new_inventory_wb.WbillId).Any(a => ((a.Discount ?? 0) - a.Amount) > 0))
                {
                    var create_write_on = SPDatabase.SPBase().ExecuteWayBill(new_inventory_wb.WbillId, 5, null).ToList().FirstOrDefault();

                    if (create_write_on != null && create_write_on.NewDocId.HasValue)
                    {
                        var wb_write_on = sp_base.WaybillList.FirstOrDefault(w => w.Id == create_write_on.NewDocId);
                        SPDatabase.SPBase().ExecuteWayBill(wb_write_on.WbillId, null, null).ToList().FirstOrDefault();
                    }
                    else
                    {
                        log_msg = $"Невдалося створити акт на введення залишків по акту інвентаризації WbillId:{new_inventory_wb.WbillId}";
                        result_exe = true;
                    }
                }

                if (sp_base.WaybillDet.Where(w => w.WbillId == new_inventory_wb.WbillId).Any(a => ((a.Discount ?? 0) - a.Amount) < 0))
                {
                    var create_write_off = SPDatabase.SPBase().ExecuteWayBill(new_inventory_wb.WbillId, -5, null).ToList().FirstOrDefault();

                    if (create_write_off != null && create_write_off.NewDocId.HasValue)
                    {
                        var wb_write_off = sp_base.WaybillList.FirstOrDefault(w => w.Id == create_write_off.NewDocId);
                        try
                        {
                            var list = new InventoryRepository().ReservedAllosition(wb_write_off.WbillId, true);
                        }

                        catch (Exception ex)
                        {
                            _log.LogException(ex, $"Помилка резервування в акті на списання товарів по акту інвернтризації | WbillId:{wb_write_off.WbillId} |");

                            result_exe = false;
                        }
                    }
                    else
                    {
                        log_msg = $"Невдалося створити акт списання залишків по акту інвентаризації WbillId:{new_inventory_wb.WbillId}";
                        result_exe = false;
                    }
                }

                if (!result_exe)
                {
                    _log.LogInfo(log_msg);
                }

                return true; // продовжуемо якщо і були помилки 
            }
        }
    }
}
