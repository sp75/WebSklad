using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tranzit_OS;
using WebApi.Api.CustomerInventory;
using WebApi.Core;

namespace WebApi.Api.OpenStore
{
    public class OpenStoreRepository: BaseRepository
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        public bool ImportKagentSales(int ka_id, int area_id, DateTime last_inventory_date, int wid)
        {
            bool rezult = true;

            try
            {
                using (var sp_base = SPDatabase.SPBase())
                {
                    var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                    var ka_sales_out = sp_base.Database.SqlQuery<SalesList>(@"SELECT 
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
FROM [BK_OS].[Tranzit_OS].[dbo].[v_Sales]
inner join Materials m on m.MatId = v_Sales.ARTID
left outer join  [BK_OS].[Tranzit_OS].[dbo].SESS_EXPORT on SESS_EXPORT.SESSID = v_Sales.SESSID and SESS_EXPORT.SYSTEMID = v_Sales.SYSTEMID and SESS_EXPORT.SAREAID = v_Sales.SAREAID
WHERE SESSEND IS NOT null and  v_Sales.SAREAID = {0} AND coalesce( m.Archived,0) = 0 and SessionStartDate > {1} and  SESS_EXPORT.SYSTEMID is null and m.TypeId in (1,5,6) and v_Sales.SALESTAG = 0 
GROUP BY [v_Sales].SESSID,v_Sales.SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, v_Sales.[SYSTEMID], m.MatId", area_id, last_inventory_date).ToList();

                    foreach (var mat_sales_item in ka_sales_out.GroupBy(g => new { g.SESSID, g.SYSTEMID, g.SessionStartDate, g.SAREAID }).ToList())
                    {

                        var wb = sp_base.WaybillList.Add(new WaybillList()
                        {
                            Id = Guid.NewGuid(),
                            WType = -5,
                            DefNum = 0,
                            OnDate = DateTime.Now,
                            Num = sp_base.GetDocNum("wb_write_off").FirstOrDefault(),
                            CurrId = 2,
                            OnValue = 1,
                            //   PersonId = DBHelper.CurrentUser.KaId,
                            WaybillMove = new WaybillMove { SourceWid = wid },
                            Nds = 0,
                            //       UpdatedBy = DBHelper.CurrentUser.UserId,
                            EntId = _enterprise?.KaId,
                            AdditionalDocTypeId = 2, //Продажі
                            Reason = $"Початок змніни: {mat_sales_item.Key.SessionStartDate}, Номер каси: {mat_sales_item.Key.SYSTEMID}",
                            Notes = $"Продажі товарів за зміну по касі {mat_sales_item.Key.SYSTEMID}"
                        });

                        sp_base.SaveChanges();

                        foreach (var item in mat_sales_item.ToList())
                        {
                                var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                                {
                                    WbillId = wb.WbillId,
                                    Num = wb.WaybillDet.Count() + 1,
                                    Amount = item.Amount,
                                    OnValue = wb.OnValue,
                                    WId = wid,
                                    Nds = wb.Nds,
                                    CurrId = wb.CurrId,
                                    OnDate = wb.OnDate,
                                    MatId = item.MatId,
                                    Price = item.Price,
                                    BasePrice = item.Price
                                });
                        }

                        sp_base.SaveChanges();

                        wb.UpdatedAt = DateTime.Now;

                        sp_base.SaveChanges();

                        if (sp_base.WaybillDet.Any(a => a.WbillId == wb.WbillId))
                        {
                            using (var tr_os_db = new Tranzit_OSEntities())
                            {
                                tr_os_db.SESS_EXPORT.Add(new SESS_EXPORT { SAREAID = mat_sales_item.Key.SAREAID, SESSID = mat_sales_item.Key.SESSID, SYSTEMID = mat_sales_item.Key.SYSTEMID, CREATED_AT = DateTime.Now });
                                tr_os_db.SaveChanges();
                            }
                        }

                            // CorrectDocument(wb, wid, $"Корегування продажу товрів по касі { mat_sales_item.Key.SYSTEMID}");
                        new ExecuteWayBill().CorrectDocument(wb.WbillId, $"Корегування продажу товрів по касі { mat_sales_item.Key.SYSTEMID}", true);

                        var list = new InventoryRepository().ReservedAllosition(wb.WbillId, true);

                        if (list.Any())
                        {
                            var message = $"Продажі товарів за зміну по касі { mat_sales_item.Key.SYSTEMID} | Не всі товари зарезервовані в документі | WbillId: {wb.WbillId} | Номенклатура | {string.Join(",", list)} | Error";
                            _log.LogInfo(message);

                            rezult = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogException(ex, $"Помилка списання товарів згідно продаж | area_id:{area_id} |");

                rezult = false;
            }

            return rezult;
        }

        public bool ImportCurrentKagentSales(int ka_id, int area_id, DateTime last_inventory_date, int wid, DateTime inventory_date)
        {
            bool rezult = true;

            try
            {
                using (var sp_base = SPDatabase.SPBase())
                {
                    var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                    var ka_sales_out = sp_base.Database.SqlQuery<SalesList>(@"SELECT 
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
FROM [BK_OS].[Tranzit_OS].[dbo].[v_Sales]
inner join Materials m on m.MatId = v_Sales.ARTID
left outer join  [BK_OS].[Tranzit_OS].[dbo].SESS_EXPORT on SESS_EXPORT.SESSID = v_Sales.SESSID and SESS_EXPORT.SYSTEMID = v_Sales.SYSTEMID and SESS_EXPORT.SAREAID = v_Sales.SAREAID
WHERE SESSEND IS null and  v_Sales.SAREAID = {0} AND coalesce( m.Archived,0) = 0 and SessionStartDate > {1}  and SessionStartDate < {2} and  SESS_EXPORT.SYSTEMID is null and m.TypeId in (1,5,6) and v_Sales.SALESTAG = 0 
GROUP BY [v_Sales].SESSID,v_Sales.SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, v_Sales.[SYSTEMID], m.MatId", area_id, last_inventory_date, inventory_date).ToList();

                    foreach (var mat_sales_item in ka_sales_out.GroupBy(g => new { g.SESSID, g.SYSTEMID, g.SessionStartDate, g.SAREAID }).ToList())
                    {
                        var wb = sp_base.WaybillList.Add(new WaybillList()
                        {
                            Id = Guid.NewGuid(),
                            WType = -5,
                            DefNum = 0,
                            OnDate = DateTime.Now,
                            Num = sp_base.GetDocNum("wb_write_off").FirstOrDefault(),
                            CurrId = 2,
                            OnValue = 1,
                            //   PersonId = DBHelper.CurrentUser.KaId,
                            WaybillMove = new WaybillMove { SourceWid = wid },
                            Nds = 0,
                            //       UpdatedBy = DBHelper.CurrentUser.UserId,
                            EntId = _enterprise?.KaId,
                            AdditionalDocTypeId = 2, //Продажі
                            Reason = $"Початок змніни: {mat_sales_item.Key.SessionStartDate}, Номер каси: {mat_sales_item.Key.SYSTEMID}",
                            Notes = $"Продажі товарів за зміну по касі {mat_sales_item.Key.SYSTEMID}"
                        });

                        sp_base.SaveChanges();

                        using (var tr_os_db = new Tranzit_OSEntities())
                        {
                            tr_os_db.SESS_EXPORT.Add(new SESS_EXPORT { SAREAID = mat_sales_item.Key.SAREAID, SESSID = mat_sales_item.Key.SESSID, SYSTEMID = mat_sales_item.Key.SYSTEMID, CREATED_AT = DateTime.Now });
                            tr_os_db.SaveChanges();
                        }

                        foreach (var item in mat_sales_item.ToList())
                        {
                            var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                            {
                                WbillId = wb.WbillId,
                                Num = wb.WaybillDet.Count() + 1,
                                Amount = item.Amount,
                                OnValue = wb.OnValue,
                                WId = wid,
                                Nds = wb.Nds,
                                CurrId = wb.CurrId,
                                OnDate = wb.OnDate,
                                MatId = item.MatId,
                                Price = item.Price,
                                BasePrice = item.Price
                            });
                        }

                        sp_base.SaveChanges();

                        wb.UpdatedAt = DateTime.Now;

                        sp_base.SaveChanges();

                        //  CorrectDocument(wb, wid, $"Корегування продажу товрів по касі { mat_sales_item.Key.SYSTEMID}");
                        new ExecuteWayBill().CorrectDocument(wb.WbillId, $"Корегування продажу товрів по касі { mat_sales_item.Key.SYSTEMID}", true);

                        var list = new InventoryRepository().ReservedAllosition(wb.WbillId, true);

                        if (list.Any())
                        {
                            var message = $"Продажі товарів за зміну по касі { mat_sales_item.Key.SYSTEMID} | Не всі товари зарезервовані в документі | WbillId: {wb.WbillId} | Номенклатура | {string.Join(",", list)} | Error";
                            _log.LogInfo(message);

                            rezult = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogException(ex, $"Помилка списання товарів згідно продаж | area_id:{area_id} |");

                rezult = false;
            }

            return rezult;
        }

        public class CorrectDetList
        {
            public int MatId { get; set; }
            public decimal Amount { get; set; }
            public decimal CorrectAmount { get; set; }
            public decimal Price { get; set; }
        }

        public void CorrectDocument(WaybillList wb_write_off, int wid,  string wb_notes)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb_det = sp_base.Database.SqlQuery<CorrectDetList>(@"select waybilldet.MatId, waybilldet.Amount,  remaain.TotalRemain, (waybilldet.Amount - remaain.TotalRemain) CorrectAmount, waybilldet.Price
from waybilldet 
outer apply (

                                   SELECT 
		                                 coalesce( sum( ActualRemain),0 ) TotalRemain
			                              
                                        FROM PosRemains pr
										inner join waybilldet wbd on wbd.posid=pr.posid
		                                WHERE pr.matid = waybilldet.MatId
                                              and pr.ondate = (select max(ondate) from posremains where posid = pr.posid ) 
                                              and (pr.remain > 0 or Ordered > 0) and pr.wid= waybilldet.WId  and ActualRemain > 0 
											  and  wbd.OnDate <= waybilldet.OnDate ) remaain

where waybilldet.WbillId = {0} and remaain.TotalRemain < waybilldet.Amount", wb_write_off.WbillId).ToList();

                if (wb_det.Any())
                {
                    var wb_in = sp_base.WaybillList.Add(new WaybillList()
                    {
                        Id = Guid.NewGuid(),
                        WType = 5,
                        DefNum = 0,
                        OnDate = wb_write_off.OnDate.AddMinutes(-1),
                        Num = sp_base.GetDocNum("wb_write_on").FirstOrDefault(),
                        CurrId = 2,
                        OnValue = 1,
                        //   PersonId = DBHelper.CurrentUser.KaId,
                        WaybillMove = new WaybillMove { SourceWid = wid, DestWId = wid },
                        Nds = 0,
                        //       UpdatedBy = DBHelper.CurrentUser.UserId,
                        EntId = wb_write_off.EntId,
                        AdditionalDocTypeId = 4, //Корегування
                        Reason = $"Документ на списання {wb_write_off.Num}",
                        Notes = wb_notes
                    });
                    sp_base.SaveChanges();

                    foreach (var item in wb_det)
                    {
                        var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = wb_in.WbillId,
                            Num = wb_in.WaybillDet.Count() + 1,
                            Amount = item.CorrectAmount,
                            OnValue = wb_in.OnValue,
                            WId = wid,
                            Nds = wb_in.Nds,
                            CurrId = wb_in.CurrId,
                            OnDate = wb_in.OnDate,
                            MatId = item.MatId,
                            Price = item.Price,
                            BasePrice = item.Price
                        });
                    }
                    sp_base.SaveChanges();

                    sp_base.DocRels.Add(new DocRels { OriginatorId = wb_write_off.Id, RelOriginatorId = wb_in.Id });

                    wb_in.UpdatedAt = DateTime.Now;

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
                }
            }
        }

        public void ImportKagentReturns(int ka_id, int area_id, DateTime last_inventory_date, int wid)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var _enterprise = sp_base.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == ka_id));

                var ka_sales_out = sp_base.Database.SqlQuery<SalesList>(@"SELECT 
  v_ReturnSales.ARTID 
 ,v_ReturnSales.ARTCODE
 ,v_ReturnSales.ARTNAME
 ,v_ReturnSales.SESSID
 ,v_ReturnSales.SAREAID
 ,v_ReturnSales.SYSTEMID
 ,v_ReturnSales.SessionStartDate
 ,m.MatId
 ,SUM(v_ReturnSales.AMOUNT) Amount
 ,SUM(v_ReturnSales.TOTAL) Total
FROM [BK_OS].[Tranzit_OS].[dbo].v_ReturnSales
inner join Materials m on m.MatId = v_ReturnSales.ARTID
left outer join  [BK_OS].[Tranzit_OS].[dbo].SESS_RETURN_EXPORT on SESS_RETURN_EXPORT.SESSID = v_ReturnSales.SESSID and SESS_RETURN_EXPORT.SYSTEMID = v_ReturnSales.SYSTEMID and SESS_RETURN_EXPORT.SAREAID = v_ReturnSales.SAREAID
WHERE SESSEND IS NOT null and  v_ReturnSales.SAREAID = {0} AND coalesce( m.Archived,0) = 0 and SessionStartDate > {1} and  SESS_RETURN_EXPORT.SYSTEMID is null and m.TypeId in (1,5,6)
GROUP BY v_ReturnSales.SESSID, v_ReturnSales.SAREAID, ARTID, ARTCODE, ARTNAME, SessionStartDate, v_ReturnSales.[SYSTEMID], m.MatId", area_id, last_inventory_date).ToList();

                foreach (var mat_sales_item in ka_sales_out.GroupBy(g => new { g.SESSID, g.SYSTEMID, g.SessionStartDate, g.SAREAID }).ToList())
                {
                    var wb = sp_base.WaybillList.Add(new WaybillList()
                    {
                        Id = Guid.NewGuid(),
                        WType = 5,
                        DefNum = 0,
                        OnDate = DateTime.Now.AddSeconds(-1),
                        Num = sp_base.GetDocNum("wb_write_on").FirstOrDefault(),
                        CurrId = 2,
                        OnValue = 1,
                        //   PersonId = DBHelper.CurrentUser.KaId,
                        WaybillMove = new WaybillMove { SourceWid = wid , DestWId = wid },
                        Nds = 0,
                        //       UpdatedBy = DBHelper.CurrentUser.UserId,
                        EntId = _enterprise?.KaId,
                        AdditionalDocTypeId = 3, //Повернення
                        Reason = $"Початок змніни: {mat_sales_item.Key.SessionStartDate}, Номер каси: {mat_sales_item.Key.SYSTEMID}",
                        Notes = $"Повернення товарів за зміну по касі {mat_sales_item.Key.SYSTEMID}"
                    });

                    sp_base.SaveChanges();

                    using (var tr_os_db = new Tranzit_OSEntities())
                    {
                        tr_os_db.SESS_RETURN_EXPORT.Add(new SESS_RETURN_EXPORT { SAREAID = mat_sales_item.Key.SAREAID, SESSID = mat_sales_item.Key.SESSID, SYSTEMID = mat_sales_item.Key.SYSTEMID, CREATED_AT = DateTime.Now });
                        tr_os_db.SaveChanges();
                    }

                    foreach (var item in mat_sales_item.ToList())
                    {
                        var wbd = sp_base.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = wb.WbillId,
                            Num = wb.WaybillDet.Count() + 1,
                            Amount = item.Amount,
                            OnValue = wb.OnValue,
                            WId = wid,
                            Nds = wb.Nds,
                            CurrId = wb.CurrId,
                            OnDate = wb.OnDate,
                            MatId = item.MatId,
                            Price = item.Total / item.Amount,
                            BasePrice = item.Total / item.Amount
                        });
                    }

                    sp_base.SaveChanges();

                    wb.UpdatedAt = DateTime.Now;

                    sp_base.SaveChanges();


                    foreach (var det_item in sp_base.WaybillDet.Where(w => w.WbillId == wb.WbillId).ToList())
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

                    wb.Checked = 1;

                    sp_base.SaveChanges();
                }
            }
        }

        public bool ImportKagentSales(Guid? id)
        {
            if (!id.HasValue)
            {
                return false;
            }

            var ka = GetOpenStoreAreaList(id).FirstOrDefault();

            ImportKagentReturns(ka.KaId, ka.OpenStoreAreaId.Value, ka.LastInventoryDate.Value, ka.WId.Value);
            var result = ImportKagentSales(ka.KaId, ka.OpenStoreAreaId.Value, ka.LastInventoryDate.Value, ka.WId.Value);
            
            return result;
        }

        public bool ImportCurrentKagentSales(Guid? id, DateTime InventoryDate)
        {
            if(!id.HasValue)
            {
                return false;
            }

            var ka = GetOpenStoreAreaList(id).FirstOrDefault();

            var result = ImportCurrentKagentSales(ka.KaId, ka.OpenStoreAreaId.Value, ka.LastInventoryDate.Value, ka.WId.Value, InventoryDate);

            return result;
        }

        public List<OpenStoreAreaList> GetOpenStoreAreaList(Guid? ka_id = null)
        {
            var sql = $@"
  SELECT KaId
      ,[Name]
      ,Id
      ,OpenStoreAreaId
      ,WId
	  ,LastInventoryDate
      ,KAU
  FROM v_Kagent
  where OpenStoreAreaId is not null and WId is not null and LastInventoryDate is not null {(ka_id.HasValue ? $" and Id = '{ka_id.Value}'" : "")}";

            return db.Database.SqlQuery<OpenStoreAreaList>(sql).ToList();
        }
    }
}