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
    [RoutePrefix("api/waybill-in")]
    [ApiTokenAuthorize]
    public class WaybillInController : BaseApiController
    {
        [HttpGet, Route("{wbill_id}/det")]
        public IHttpActionResult GetWaybilldet(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_WaybillDet.Where(w=> w.WbillId == wbill_id).ToList());
            }
        }

    }
}
