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

                var mat_remain = new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value).Where(w => w.TypeId != null).Select(s => new MaterialsOnWh
                {
                    Artikul = s.Artikul,
                    AvgPrice = s.AvgPrice,
                    CurRemain = s.CurRemain,
                    GrpName = s.GrpName,
                    MatId = s.MatId,
                    MatName = s.MatName,
                    MsrName = s.MsrName,
                    OpenStoreId = s.MatId,
                    Remain = s.Remain,
                    TypeId = s.TypeId,
                    Rsv = s.Rsv,
                    SumRemain = s.SumRemain,
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

        public class MoveToStoreWarehouseWbList
        {
            public int WbillId { get; set; }
            public DateTime ShipmentDate { get; set; }
        }
    }
}
