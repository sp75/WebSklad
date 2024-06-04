using SP.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Core;

namespace WebApi.Controllers
{
    [ApiTokenAuthorize]
    [RoutePrefix("api/shops")]
    public class KagentController : BaseApiController
    {

        [HttpGet, Route("info")]
        public IHttpActionResult GetCustomerInfo()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.KagentList.FirstOrDefault(w => w.Id == Context.Token));
            }
        }

        [HttpGet, Route("list")]
        public IHttpActionResult List()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_Kagent.Where(w => w.KType == 4 && w.WId != null && w.Archived == 0 && w.Id != Context.Token).OrderBy(o=> o.Name).Select(s => new
                {
                    s.Id,
                    s.WId,
                    s.Name,
                    s.FullFactADDR
                }).ToList());
            }
        }
    }
}
