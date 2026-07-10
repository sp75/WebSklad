using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Api.CustomerInventory;
using WebApi.Controllers;
using WebApi.Core;

namespace WebApi.Api.Manufacture
{
    public class ManufactureRepository : BaseRepository
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        public void AutoProduceSales(int ka_id, int area_id, DateTime last_inventory_date, int wid)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                var ka_sales_recipe = sp_base.Database.SqlQuery<SalesList>(@"SELECT 
  v_Sales.ARTID 
 ,v_Sales.ARTCODE
 ,v_Sales.ARTNAME
 ,v_Sales.SESSID
 ,v_Sales.SAREAID
 ,v_Sales.SYSTEMID
 ,v_Sales.SessionStartDate
 ,m.MatId
 ,SUM(v_Sales.AMOUNT) Amount
 ,SUM(v_Sales.TOTAL) Total
 ,AVG(v_Sales.PRICE) Price
 ,mr.RecId
FROM [BK_OS].[Tranzit_OS].[dbo].[v_Sales]
inner join Materials m on m.MatId = v_Sales.ARTID
inner join MatRecipe mr on mr.MatId = m.MatId.
left outer join  [BK_OS].[Tranzit_OS].[dbo].SESS_AUTOPRODUCE_EXPORT e on e.SESSID = v_Sales.SESSID and e.SYSTEMID = v_Sales.SYSTEMID and e.SAREAID = v_Sales.SAREAID and e.ARTCODE = v_Sales.ARTID 
WHERE mr.AutoProduce = 1 AND SESSEND IS NOT null and  v_Sales.SAREAID = {0} AND coalesce( m.Archived,0) = 0 and SessionStartDate > {1} and  SESS_EXPORT.SYSTEMID is null and m.TypeId = 1 and v_Sales.SALESTAG = 0 
GROUP BY [v_Sales].SESSID,v_Sales.SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, v_Sales.[SYSTEMID], m.MatId", area_id, last_inventory_date).ToList();
            }

        }



        public bool ManufactureProduct_old(int rec_id, int ka_id, int wid, decimal amount, string notes, int user_id)
        {
            bool result = true;

            using (var _db = SPDatabase.SPBase())
            {
                var _enterprise = _db.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                var wb = _db.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = -20,
                    OnDate = DateTime.Now,
                    Num = _db.GetDocNum("wb_make").FirstOrDefault(),
                    EntId = _enterprise.KaId,
                    CurrId = 2,
                    OnValue = 1,
                    PersonId = ka_id,
                    KaId = ka_id,
                    WayBillMake = new WayBillMake { SourceWId = wid, RecId = rec_id, Amount = amount, PersonId = ka_id },
                    Notes = notes,
                    UpdatedBy = user_id
                });
                _db.SaveChanges();

                var r = _db.GetRecipe(wb.WbillId).ToList();
                foreach (var det in _db.WaybillDet.Where(a => a.WbillId == wb.WbillId).ToList())
                {
                    det.WId = wid;
                }
                _db.SaveChanges();

                if (_db.WaybillDet.Any(a => a.WbillId == wb.WbillId))
                {
                    wb.UpdatedAt = DateTime.Now;

                    _db.SaveChanges();


                    var list = new InventoryRepository().ReservedAllosition(wb.WbillId, true);

                    if (list.Any())
                    {
                        var message = $"Склад: {wid} |  Віддалене виготовлення продукції | Не всі товари зарезервовані в документі | rec_id: {rec_id} | Сировина | {string.Join(",", list)} | Error";
                        _log.LogInfo(message);

                        _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);
                        result = false;
                    }
                    else
                    {
                        try
                        {
                            var ex_result = db.ExecuteWayBill(wb.WbillId, null, ka_id).ToList().FirstOrDefault();
                            var write_on = db.WaybillList.FirstOrDefault(w => w.Id == ex_result.NewDocId);
                            var result2 = db.ExecuteWayBill(write_on.WbillId, null, ka_id).ToList().FirstOrDefault();
                        }
                        catch
                        {
                            var message = $"Склад: {wid} |  Віддалене виготовлення продукції | Не вдалося провести документ та відванажити готову продукцію | Error";
                            _log.LogInfo(message);

                            _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);
                        
                            result = false;
                        }
                    }
                }
                else
                {
                    _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);
                }
            }

            return result;
        }

        public OperationResult ManufactureProduct(int rec_id, int ka_id, int wid, decimal amount, string notes, int user_id)
        {
            using (var _db = SPDatabase.SPBase())
            {
                var _enterprise = _db.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                var wb = _db.WaybillList.Add(new WaybillList()
                {
                    Id = Guid.NewGuid(),
                    WType = -20,
                    OnDate = DateTime.Now,
                    Num = _db.GetDocNum("wb_make").FirstOrDefault(),
                    EntId = _enterprise.KaId,
                    CurrId = 2,
                    OnValue = 1,
                    PersonId = ka_id,
                    KaId = ka_id,
                    WayBillMake = new WayBillMake { SourceWId = wid, RecId = rec_id, Amount = amount, PersonId = ka_id },
                    Notes = notes,
                    UpdatedBy = user_id
                });
                _db.SaveChanges();

                var r = _db.GetRecipe(wb.WbillId).ToList();
                foreach (var det in _db.WaybillDet.Where(a => a.WbillId == wb.WbillId).ToList())
                {
                    det.WId = wid;
                }
                _db.SaveChanges();

                if (_db.WaybillDet.Any(a => a.WbillId == wb.WbillId))
                {
                    wb.UpdatedAt = DateTime.Now;
                    _db.SaveChanges();

                    var list = new InventoryRepository().ReservedAllosition(wb.WbillId, true);

                    if (list.Any())
                    {
                        var logMessage = $"Склад: {wid} | Віддалене виготовлення продукції | Не всі товари зарезервовані в документі | rec_id: {rec_id} | Сировина | {string.Join(",", list)} | Error";
                        _log.LogInfo(logMessage);

                        _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);

                        // Зрозуміле повідомлення для користувача
                        return OperationResult.Fail($"Не всі товари зарезервовані в документі. Нестача по сировині: {string.Join(", ", list)}");
                    }
                    else
                    {
                        try
                        {
                            var ex_result = db.ExecuteWayBill(wb.WbillId, null, ka_id).ToList().FirstOrDefault();
                            var write_on = db.WaybillList.FirstOrDefault(w => w.Id == ex_result.NewDocId);
                            var result2 = db.ExecuteWayBill(write_on.WbillId, null, ka_id).ToList().FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            var logMessage = $"Склад: {wid} | Віддалене виготовлення продукції | Не вдалося провести документ та відвантажити готову продукцію | Error: {ex.Message}";
                            _log.LogInfo(logMessage);

                            _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);

                            return OperationResult.Fail("Не вдалося провести документ та відвантажити готову продукцію.");
                        }
                    }
                }
                else
                {
                    _db.DeleteWhere<WaybillList>(w => w.WbillId == wb.WbillId);
                    return OperationResult.Fail("Специфікація документу порожня.");
                }
            }

            return OperationResult.Ok("Продукцію успішно виготовлено");
        }

        public class SalesList
        {
            public int ARTID { get; set; }
            public int ARTCODE { get; set; }
            public string ARTNAME { get; set; }
            public int SESSID { get; set; }
            public int SAREAID { get; set; }
            public int SYSTEMID { get; set; }
            public DateTime SessionStartDate { get; set; }
            public int MatId { get; set; }
            public decimal Amount { get; set; }
            public decimal Total { get; set; }
            public string GrpName { get; set; }
            public string UNITNAME { get; set; }
            public decimal Price { get; set; }
            public DateTime OnDate { get; set; }
            public int RecId { get; set; }
        }
    }
}