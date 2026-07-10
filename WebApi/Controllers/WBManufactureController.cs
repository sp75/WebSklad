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
using WebApi.Api.Manufacture;
using WebApi.Controllers.Models;
using WebApi.Core;


namespace WebApi.Controllers
{
    [RoutePrefix("api/manufacture-product")]
    [ApiTokenAuthorize]
    public class WBManufactureController : BaseApiController
    {
        [HttpPost, Route("list")]
        public IHttpActionResult GetWaybill(FilterWb req)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_ManufacturingProducts.Where(w => w.OnDate >= req.start_date && w.OnDate < req.end_date && w.WType == -20 && w.SourceWId == context_ka.WId.Value).Select(s => new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.KaName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    s.MatName,
                    s.AmountOut,
                    s.Price
                }).ToList();

                return Ok(wb);
            }
        }

        [HttpGet, Route("{wbill_id}/det")]
        public IHttpActionResult GetWaybillDet(int wbill_id)
        {
            var det = db.v_ManufacturingProductsDet.Where(w => w.WbillId == wbill_id).Select(s1 => new
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
                s1.GroupName,
                s1.Notes,
                s1.WhName,
                s1.Total
            }).ToList();

            return Ok(det);
        }

        [HttpGet, Route("recipe-list")]
        public IHttpActionResult GetMatRecipe()
        {
            var det = db.KagentMatRecipe.Where(w => w.KaId == context_ka.KaId).Select(s => new
            {
                s.RecId,
                s.MatRecipe.Materials.Name,
                s.MatRecipe.Materials.Artikul
            }).ToList();

            return Ok(det);
        }

        [HttpPost, Route("produce")]
        public IHttpActionResult ManufactureProduct(ManufactureProductRequest req)
        {
            var rep = new ManufactureRepository();
            var result = rep.ManufactureProduct(req.RecId, context_ka.KaId, context_ka.WId.Value, req.Amount, req.Notes, system_user_id);

            return Ok(result); // Тепер у JSON повернеться { success: true/false, message: "..." }
        }
    }
}