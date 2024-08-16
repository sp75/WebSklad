using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerReturns;
using WebApi.Api.CustomerSales;
using WebApi.Api.OpenStore;
using WebApi.Controllers.Models;
using WebApi.Core;
namespace WebApi.Controllers
{
    [RoutePrefix("api/customer-orders")]
    public class CustomerOrdersController : BaseApiController
    {
        [ApiTokenAuthorize]
        [HttpGet, Route("{WbillId}/close")]
        public IHttpActionResult CloseOrder(int WbillId)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                sp_base.DeleteWhere<WaybillDet>(w => w.WbillId == WbillId && w.Checked != 1);

                var wbl = sp_base.WaybillList.FirstOrDefault(w => w.WbillId == WbillId);

                if (wbl.Checked == 0)
                {
                    var new_id = sp_base.ExecuteWayBill(WbillId, null, null).ToList().FirstOrDefault();

                    return Ok(new_id?.NewDocId);
                }
                else return null;
            }
        }
    }
}