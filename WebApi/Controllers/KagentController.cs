using SP.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.OpenStore;
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

        [HttpGet, Route("cash-balance")]
        public IHttpActionResult GetCashBalance()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var ka = sp_base.Kagent.FirstOrDefault(w => w.Id == Context.Token);

                return Ok(sp_base.v_MoneySaldo.Where(w=> w.KaId == ka.KaId).FirstOrDefault()) ;
            }
        }

        [HttpGet, Route("cash-current-sesion")]
        public IHttpActionResult GetTotalCashInCurrentSesion()
        {
            return Ok(new OpenStoreRepository().GetTotalCashInCurrentSesion(Context.Token));
        }


        [HttpGet, Route("list")]
        public IHttpActionResult List()
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                return Ok(sp_base.v_Kagent.Where(w => w.KType == 4 && w.WId != null && w.Archived == 0 && w.Id != Context.Token).OrderBy(o=> o.Name).Select(s => new
                {
                    s.KaId,
                    s.Id,
                    s.WId,
                    s.Name,
                    s.FullFactADDR
                }).ToList());
            }
        }
    }
}
