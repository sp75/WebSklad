using SkladEngine.DBFunction;
using SkladEngine.ExecuteDoc;
using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerRemain;
using WebApi.Api.CustomerSales;
using WebApi.Controllers.Models;
using WebApi.Core;


namespace WebApi.Controllers
{
    [RoutePrefix("api/warehouse")]
    public class WarehouseController : BaseApiController
    {
        private readonly Log4netLogger _log = new Log4netLogger("ErrorNotification");

        [HttpGet, Route("test")]
        public bool Test()
        {
            _log.LogInfo("Ok");
         /*   using (var dd = File.Open(@"C:\inetpub\wwwroot\WebSklad\WebApi\WebApiLog\1.txt", FileMode.OpenOrCreate))
            {
                ;
            }*/

            return true;
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("remain")]
        public IHttpActionResult GetRemainInWh()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);

                var ka_sales_out = new CustomerSalesRepository().GetCurrentSales(Context.Token.Value);
                var ka_return_sales = new CustomerSalesRepository().GetCurrentReturns(Context.Token.Value);
                var ka_price = sp_base.v_KagentMaterilPrices.Where(w => w.KaId == ka.KaId).Select(s=> new { s.MatId, s.Price}).ToList();

                var mat_remain = new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value).Where(w => w.TypeId != null).Select(s => new MaterialsOnWh
                {
                    Artikul = s.Artikul,
                    AvgPrice = ka_price.Where(w => w.MatId == s.MatId).Select(sp => sp.Price).FirstOrDefault(), //s.AvgPrice,
                    CurRemain = s.CurRemain,
                    GrpName = s.GrpName,
                    MatId = s.MatId,
                    MatName = s.MatName,
                    MsrName = s.MsrName,
                    OpenStoreId = s.MatId,
                    Remain = s.Remain,
                    TypeId = s.TypeId,
                    Rsv = s.Rsv,
                    SumRemain = Math.Round((ka_price.Where(w => w.MatId == s.MatId).Select(sp => sp.Price).FirstOrDefault() ?? 0) * s.Remain, 2),
                    AmountSold = ka_sales_out.Where(w => w.MatId == s.MatId).Select(ss => ss.Amount).FirstOrDefault() - ka_return_sales.Where(w => w.MatId == s.MatId).Select(ss => ss.Amount).FirstOrDefault()
                }).ToList();

                foreach (var item in ka_sales_out)
                {
                    if (!mat_remain.Any(a => a.MatId == item.MatId))
                    {
                        mat_remain.Add(new MaterialsOnWh
                        {
                            AmountSold = item.Amount,
                            Artikul = item.ARTCODE.ToString(),
                            MatId = item.MatId,
                            MatName = item.ARTNAME,
                            GrpName = item.GrpName,
                            MsrName = item.UNITNAME,
                            Remain = 0,
                            CurRemain = 0,
                            Rsv = 0,
                            OpenStoreId = item.ARTID
                        });
                    }
                }

                foreach (var item in ka_return_sales)
                {
                    if (!mat_remain.Any(a => a.MatId == item.MatId))
                    {
                        mat_remain.Add(new MaterialsOnWh
                        {
                            AmountSold = item.Amount * -1,
                            Artikul = item.ARTCODE.ToString(),
                            MatId = item.MatId,
                            MatName = item.ARTNAME,
                            GrpName = item.GrpName,
                            MsrName = item.UNITNAME,
                            Remain = 0,
                            CurRemain = 0,
                            Rsv = 0,
                            OpenStoreId = item.ARTID
                        });
                    }
                }

                return Ok(mat_remain);
            }
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("add-doc/{wbill_id}")]
        public IHttpActionResult AddDocument(int wbill_id)
        {
            var new_doc = new ExecuteWayBill().MoveToStoreWarehouse(wbill_id, true);

            return Ok(new_doc.HasValue);
        }

        [HttpGet, Route("auto-move-material-to-store-wh")]
        public IHttpActionResult MoveToStoreService()
        {
            var wb_out_list = db.Database.SqlQuery<MoveToStoreWarehouseWbList>(@"
            SELECT wb.WbillId, wb.ShipmentDate
            FROM v_WayBillOut wb
            inner join[dbo].v_Kagent on wb.KaId = v_Kagent.KaId
            where [OpenStoreAreaId] is not null and WId is not null and LastInventoryDate is not null and wb.IsDelivered = 0 and InTransit = 1
                  and wb.ShipmentDate < GETDATE() and wb.ShipmentDate > v_Kagent.[LastInventoryDate] and wb.WType = -1 and wb.Checked = 1").ToList();

            var _repo = new ExecuteWayBill();
            foreach (var item in wb_out_list)
            {
                _repo.MoveToStoreWarehouse(item.WbillId, true);
            }

            return Ok(wb_out_list);
        }

        [HttpGet, Route("auto-insert-external-waybill-to-store-wh")]
        public IHttpActionResult MoveExternalWbOutToStoreService()
        {
            var wb_out_list = db.Database.SqlQuery<ExtWaybillList>(@"SELECT wb_s.[Id]
      ,wb_s.[WbillId]
      ,wb_s.[Num]
      ,wb_s.[OnDate]
      ,wb_s.[Reason]
      ,wb_s.[Checked]
      ,wb_s.[WType]
      ,wb_s.[Nds]
      ,wb_s.[PersonId]
      ,wb_s.[SummPay]
      ,wb_s.[Notes]
      ,wb_s.[SummInCurr]
      ,[KaName]
      ,[EntName]
      ,wb_s.[ShipmentDate]
      ,[CarNumber]
      ,[CarName]
      ,[DriverName]
      ,[RouteName]
      ,ka_t.[KaId]
      ,ka_t.WId
      ,[KaArchived]
      ,[DefTotalAmount]
      ,[ExTotalAmount]
      ,[KagentId]
      ,wb_s.[AdditionalDocTypeId]
      ,wb_s.[DeliveredWaybillId]
      ,wh.Name WhName
  FROM [MIRONOVKA].[sp_base].[dbo].[v_WayBillBase] wb_s
  INNER JOIN [sp_base].[dbo].Kagent as ka_t on ka_t.Id = wb_s.[KagentId]
  INNER JOIN [sp_base].[dbo].Warehouse as wh on wh.WId = ka_t.WId
  left outer join [sp_base].[dbo].WaybillList as wb_target on wb_target.Id = wb_s.Id
  where wb_s.[OnDate] > GETDATE()-2 and wb_s.ShipmentDate < GETDATE() and wb_s.WType = -1 and wb_s.[Checked] = 1 and wb_target.WbillId is null
        and exists (SELECT ExpeditionDet.* FROM [MIRONOVKA].[sp_base].[dbo].ExpeditionDet
		           inner join [MIRONOVKA].[sp_base].[dbo].Expedition on Expedition.Id = ExpeditionDet.ExpeditionId
                   WHERE  ExpeditionDet.WbillId = wb_s.WbillId and ExpeditionDet.Checked = 1 and Expedition.Checked = 1)
  order by [OnDate]").ToList();

            foreach (var item in wb_out_list)
            {
                using (var db = SPDatabase.SPBase())
                {
                    var wb_in = db.WaybillList.Add(new WaybillList()
                    {
                        Id = item.Id,
                        WType = 1,
                        KaId = 4257, //ТОВ "АГРОПЕРЕРОБНИЙ КОМПЛЕКС-МИРОНІВКА"
                        DefNum = 0,
                        OnDate = DateTime.Now,
                        Num = db.GetDocNum("wb_in").FirstOrDefault(),
                        CurrId = 2,
                        OnValue = 1,
                        // PersonId = wb_out.PersonId,
                        Nds = 0,
                        //    UpdatedBy = wb_out.UpdatedBy,
                        UpdatedAt = DateTime.Now,
                        EntId = 2605,
                        PTypeId = 2,
                        Notes = item.WhName,
                        Reason = $"Оприбуткування на склад {item.WhName} згідно видаткової накладної №{item.Num}"
                    });

                    db.SaveChanges();

                    var wb_out_det = db.Database.SqlQuery<ExtWaybillDet>(@"SELECT 
       PosId
      ,m_t.[MatId]
      ,[Amount]
      ,[Price]
      ,WaybillDet.[Num]
  FROM [MIRONOVKA].[sp_base].[dbo].[WaybillDet]
  inner join  [MIRONOVKA].[sp_base].[dbo].[Materials] as m on m.MatId = WaybillDet.MatId 
  inner join  [sp_base].[dbo].[Materials] as m_t on m_t.Artikul = m.Artikul 
  where WaybillDet.WbillId = {0} and (m.Archived is null or m.Archived = 0 ) and (m_t.Archived is null or m_t.Archived = 0 )", item.WbillId).ToList();

                    foreach (var det_item in wb_out_det)
                    {
                        var _wbd = db.WaybillDet.Add(new WaybillDet()
                        {
                            WbillId = wb_in.WbillId,
                            OnDate = wb_in.OnDate,
                            MatId = det_item.MatId,
                            Discount = 0,
                            Nds = wb_in.Nds,
                            CurrId = wb_in.CurrId,
                            OnValue = wb_in.OnValue,
                            Num = det_item.Num,
                            PosKind = 0,
                            DiscountKind = 0,
                            Amount = det_item.Amount,
                            BasePrice = det_item.Price,
                            Price = det_item.Price,
                            WId = item.WId,
                        });

                    }
                    db.SaveChanges();

                    foreach (var det_item in db.WaybillDet.Where(w => w.WbillId == wb_in.WbillId).ToList())
                    {
                        db.WMatTurn.Add(new WMatTurn
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

                    db.SaveChanges();
                }
            }

            return Ok(true);
        }

        public class MoveToStoreWarehouseWbList
        {
            public int WbillId { get; set; }
            public DateTime ShipmentDate { get; set; }
        }

        public class ExtWaybillList
        {
            public int? WId { get; set; }
            public DateTime? ShipmentDate { get; set; }
            public DateTime? ReportingDate { get; set; }
            public Guid? ReceiptId { get; set; }
            public string CarNumber { get; set; }
            public string CarName { get; set; }
            public string DriverName { get; set; }
            public string RouteName { get; set; }
            public TimeSpan? RouteDuration { get; set; }
            public long? RouteId { get; set; }
            public Guid? CarId { get; set; }
            public int? CustomerId { get; set; }
            public int? DriverId { get; set; }
            public int? KaId { get; set; }
            public decimal? DefTotalAmount { get; set; }
            public decimal? ExTotalAmount { get; set; }
            public string KagentGroupName { get; set; }
            public Guid? KagentId { get; set; }
            public string EntName { get; set; }
            public Guid Id { get; set; }
            public int WbillId { get; set; }
            public string Num { get; set; }
            public DateTime OnDate { get; set; }
            public int? CurrId { get; set; }
            public string Reason { get; set; }
            public decimal? SummAll { get; set; }
            public decimal? SummPay { get; set; }
            public string Notes { get; set; }
            public decimal? SummInCurr { get; set; }
            public string KaName { get; set; }
            public string KaFullName { get; set; }
            public string PersonName { get; set; }
            public int? KType { get; set; }
            public string Received { get; set; }
            public DateTime? ToDate { get; set; }
            public int? EntId { get; set; }
            public int? DeliveredWaybillId { get; set; }
            public string WhName { get; set; }
        }

        public class ExtWaybillDet
        {
            public string Notes { get; set; }
            public decimal? AvgInPrice { get; set; }
            public int PosId { get; set; }
            public int WbillId { get; set; }
            public int MatId { get; set; }
            public int? WId { get; set; }
            public decimal Amount { get; set; }
            public decimal? Price { get; set; }
            public decimal? Discount { get; set; }
            public decimal? Nds { get; set; }
            public DateTime? OnDate { get; set; }
            public int Num { get; set; }
            public int? Checked { get; set; }
            public decimal? OnValue { get; set; }
            public decimal? Total { get; set; }
            public decimal? BasePrice { get; set; }
            public DateTime? Expires { get; set; }
            public int? PosKind { get; set; }
            public int? PosParent { get; set; }
            public int? MsrUnitId { get; set; }
            public int? DiscountKind { get; set; }
        }
    }
}
