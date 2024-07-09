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
using WebApi.Api.CustomerRemain;
using WebApi.Api.CustomerSales;
using WebApi.Controllers.Models;
using WebApi.Core;


namespace WebApi.Controllers
{
    [RoutePrefix("api/warehouse")]
    [ApiTokenAuthorize]
    public class WarehouseController : BaseApiController
    {
        [HttpGet, Route("remain")]
        public IHttpActionResult GetRemainInWh()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);
                var ka_sales_out = new CustomerSalesRepository().GetCurrentSales(Context.Token.Value);
                
                var mat_remain = new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value).Select(s => new MaterialsOnWh
                {
                    Artikul = s.Artikul,
                    AvgPrice = s.AvgPrice,
                    CurRemain = s.CurRemain,
                    GrpName = s.GrpName,
                    MatId = s.MatId,
                    MatName = s.MatName,
                    MsrName = s.MsrName,
                    OpenStoreId = s.OpenStoreId,
                    Remain = s.Remain,
                    TypeId = s.TypeId,
                    Rsv = s.Rsv,
                    SumRemain = s.SumRemain,
                    AmountSold = ka_sales_out.Where(w => w.MatId == s.MatId).Select(ss => ss.Amount).FirstOrDefault()
                }).ToList();

                foreach (var item in ka_sales_out)
                {
                    if(!mat_remain.Any(a=> a.MatId == item.MatId))
                    {
                        mat_remain.Add(new MaterialsOnWh
                        {
                            AmountSold = item.Amount,
                            Artikul = item.ARTCODE.ToString(),
                            MatId = item.MatId,
                            MatName = item.ARTNAME,
                            GrpName = item.GrpName,
                            MsrName = item.UNITNAME
                        });
                    }
                }

                  

                return Ok(mat_remain);
            }
        }

        [HttpGet, Route("add-doc/{wbill_id}")]
        public IHttpActionResult AddDocument(int wbill_id)
        {
            var new_doc = new ExecuteWayBill().MoveToStoreWarehouse(wbill_id, true);

            return Ok(new_doc.HasValue);
        }

    }


}
