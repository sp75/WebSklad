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

                return Ok(new MaterialRemain(0).GetMaterialsOnWh(ka.WId.Value));
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
