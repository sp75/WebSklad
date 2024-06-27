﻿using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tranzit_OS;
using WebApi.Api.CustomerInventory;
using WebApi.Api.CustomerMove;
using WebApi.Controllers.Models;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/open-store")]
    public class OpenStoreController :  BaseApiController
    {
        [HttpGet, Route("import-sales")]
        public void ImportSales()
        {
            var ka_list = db.Database.SqlQuery<KagentList>(@"select * from
(
  SELECT [KaId]
      ,[Name]
      ,[Id]
      ,[OpenStoreAreaId]
      ,WId
	  , (SELECT MAX(wl.ondate) FROM v_WaybillInventory wl WHERE wl.FromWId = [Kagent].WId ) LastInventoryDate
  FROM [dbo].[Kagent]
  where [OpenStoreAreaId] is not null and WId is not null
  )x
where x.LastInventoryDate is not null").ToList();

            foreach (var k_item in ka_list)
            {
                var _enterprise = db.Kagent.FirstOrDefault(w => w.KType == 3 && w.Deleted == 0 && (w.Archived == null || w.Archived == 0) && w.EnterpriseWorker.Any(a => a.WorkerId == k_item.KaId));


                var ka_sales = db.Database.SqlQuery<SalesList>(@"SELECT 
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
GROUP BY [v_Sales].SESSID,v_Sales.SAREAID, ARTID, ARTCODE, ARTNAME,SessionStartDate, v_Sales.[SYSTEMID], m.MatId", k_item.OpenStoreAreaId, k_item.LastInventoryDate).ToList();
                using (var sp_base = SPDatabase.SPBase())
                {
                    foreach (var mat_sales_item in ka_sales.GroupBy(g => new { g.SESSID, g.SYSTEMID, g.SessionStartDate, g.SAREAID }).ToList())
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
                            WaybillMove = new WaybillMove { SourceWid = k_item.WId.Value },
                            Nds = 0,
                            //       UpdatedBy = DBHelper.CurrentUser.UserId,
                            EntId = _enterprise?.KaId,
                            AdditionalDocTypeId = 2, //Продажі
                            Reason = $"Почток змніни: {mat_sales_item.Key.SessionStartDate}, Номер каси: {mat_sales_item.Key.SYSTEMID}",
                            Notes = "Списання товарів за зміну по касі"
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
                                WId = k_item.WId.Value,
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

                        using (var tr_os_db = new Tranzit_OSEntities())
                        {
                            tr_os_db.SESS_EXPOTR.Add(new SESS_EXPOTR { SAREAID = mat_sales_item.Key.SAREAID, SESSID = mat_sales_item.Key.SESSID, SYSTEMID = mat_sales_item.Key.SYSTEMID });
                            tr_os_db.SaveChanges();
                        }
                            
                    }
                }
            }
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
        }

        public class KagentList
        {
            public int KaId { get; set; }
            public int? OpenStoreAreaId { get; set; }
            public DateTime? LastInventoryDate { get; set; }
            public int? WId { get; set; }
        }
    }
}