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
    [RoutePrefix("api/material")]
    public class MaterialsController : BaseApiController
    {
        [ApiTokenAuthorize]
        [HttpGet, Route("find-by-artikul")]
        public IHttpActionResult FindMatByArtikul(string artikul)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.Materials.Where(w=> w.Artikul == artikul).Select(s=> new { s.MatId, s.Name, s.TypeId}).FirstOrDefault());
            }
        }
    }
}
