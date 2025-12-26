using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerEncashment;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/encashment")]
    [ApiTokenAuthorize]
    public class EncashmentController : BaseApiController
    {
        [HttpGet, Route("list")]
        public IHttpActionResult GetEncashmentList()
        {
            return Ok(new EncashmentRepository().GetEncashment(ka.KaId));
        }

        [HttpGet, Route("{id}")]
        public IHttpActionResult GetEncashmentItem(Guid id)
        {
            return Ok(new EncashmentRepository().GetEncashmentItem(id));
        }

        [HttpPost, Route("")]
        public bool SetEncashment(Encashment req)
        {
            var rep = new EncashmentRepository();

            return rep.SetEncashment(ka.KaId, req);
        }

        [HttpPost, Route("{id}/del")]
        public bool DelEncashment(Guid id)
        {
            var rep = new EncashmentRepository();

            return rep.DelEncashment(id);
        }
    }
}
