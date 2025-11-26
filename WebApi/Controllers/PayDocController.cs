using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Api.CustomerPayments;
using WebApi.Core;

namespace WebApi.Controllers
{
    [RoutePrefix("api/pay-doc")]
    [ApiTokenAuthorize]
    public class PayDocController : BaseApiController
    {
        [ApiTokenAuthorize]
        [HttpGet, Route("list/{doc_type}")]
        public IHttpActionResult GetPayDocList(int doc_type)
        {
            var rep = new CustomerPaymentsRepository();

            return Ok(rep.GetPayDocList(Context.Token, doc_type));
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("{pay_doc_id}")]
        public IHttpActionResult GetPayDoc(int pay_doc_id)
        {
            var rep = new CustomerPaymentsRepository();

            return Ok(rep.GetPayDoc(pay_doc_id));
        }

        [ApiTokenAuthorize]
        [HttpGet, Route("charge-type-list")]
        public IHttpActionResult GetChargeType()
        {
            var rep = new CustomerPaymentsRepository();

            return Ok(rep.GetChargeType());
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("create-money-move")]
        public IHttpActionResult CreateMoneyMove(CreateMoneyMoveRequest req)
        {
            var rep = new CustomerPaymentsRepository();

            return Ok(rep.MoveMoney(Context.Token, req.Total));
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("create-additional-costs")]
        public IHttpActionResult CreateAdditionalCosts(AdditionalCostsRequest req)
        {
            var rep = new CustomerPaymentsRepository();

            return Ok(rep.NewPayDoc(Context.Token, req.Total, req.ctypeid, req.ka_id, req.notes));
        }


        public class CreateMoneyMoveRequest
        {
            public decimal Total { get; set; }
        }

        public class AdditionalCostsRequest
        {
            public decimal Total { get; set; }
            public int ctypeid { get; set; }
            public int? ka_id { get; set; }
            public string notes { get; set; }
        }
    }
}
