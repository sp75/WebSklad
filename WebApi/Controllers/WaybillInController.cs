﻿using SkladEngine.DBFunction;
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
        [HttpGet, Route("{wbill_id}")]
        public IHttpActionResult GetWaybill(int wbill_id)
        {
            using (var sp_base = SPDatabase.SPBase())
            {
                var wb = sp_base.v_WayBillIn.Where(w=> w.WbillId == wbill_id).Select(s=> new
                {
                    s.WbillId,
                    s.Num,
                    s.OnDate,
                    s.SummInCurr,
                    s.KaName,
                    s.EntName,
                    s.Checked,
                    s.Id,
                    s.Notes,
                    s.Reason,
                    Details = sp_base.v_WayBillInDet.Where(w => w.WbillId == s.WbillId).Select(s1 => new
                    {
                        s1.PosId,
                        s1.OnDate,
                        s1.MatName,
                        s1.MatId,
                        s1.MsrName,
                        s1.Amount,
                        s1.BasePrice,
                        s1.Price,
                        s1.Discount,
                        s1.Artikul,
                        s1.GrpName,
                        s1.Notes,
                        s1.WhName,
                        s1.Wid
                    })
                }).FirstOrDefault();

                return Ok(wb);
            }
        }

    }
}
