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
        public void ImportKagentSales(int ka_id, int area_id, DateTime last_inventory_date, int wid)
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
FROM [SERVER_OS].[Tranzit_OS].[dbo].[v_Sales]
inner join Materials m on m.OpenStoreId = v_Sales.ARTID
left outer join  [SERVER_OS].[Tranzit_OS].[dbo].SESS_EXPOTR on SESS_EXPOTR.SESSID = v_Sales.SESSID and SESS_EXPOTR.SYSTEMID = v_Sales.SYSTEMID and SESS_EXPOTR.SAREAID = v_Sales.SAREAID
WHERE SESSEND IS NOT null and  v_Sales.SAREAID = {0} AND coalesce( m.Archived,0) = 0 and SessionStartDate > {1} and  SESS_EXPOTR.SYSTEMID is null and m.TypeId in (1,5)
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
                        Reason = $"Почток змніни: {mat_sales_item.Key.SessionStartDate}, Номер каси: {mat_sales_item.Key.SYSTEMID}",
                        Notes = "Списання товарів за зміну по касі"
                    });

                    sp_base.SaveChanges();

                    using (var tr_os_db = new Tranzit_OSEntities())
                    {
                        tr_os_db.SESS_EXPOTR.Add(new SESS_EXPOTR { SAREAID = mat_sales_item.Key.SAREAID, SESSID = mat_sales_item.Key.SESSID, SYSTEMID = mat_sales_item.Key.SYSTEMID });
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

                    var list = new InventoryRepository().ReservedAllosition(wb.WbillId, false);
                }
            }
        }

        public void ImportKagentSales(Guid? id)
        {
            var ka = db.Database.SqlQuery<OpenStoreAreaList>(@"select * from
(
  SELECT [KaId]
      ,[Name]
      ,[Id]
      ,[OpenStoreAreaId]
      ,WId
	  , (SELECT MAX(wl.ondate) FROM v_WaybillInventory wl WHERE wl.FromWId = [Kagent].WId ) LastInventoryDate
  FROM [dbo].[Kagent]
  where [OpenStoreAreaId] is not null and WId is not null and Kagent.Id= {0}
  )x
where x.LastInventoryDate is not null", id).FirstOrDefault();

            ImportKagentSales(ka.KaId, ka.OpenStoreAreaId.Value, ka.LastInventoryDate.Value, ka.WId.Value);
        }
    }
}