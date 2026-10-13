using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tranzit_OS;
using WebApi.Core;
namespace WebApi.Controllers
{
   
    [RoutePrefix("api/shift-cashiers")]
    public class ShiftCashiersController : BaseApiController
    {
        [ApiTokenAuthorize]
        [HttpGet, Route("{shift_id}")]
        public IHttpActionResult GetShiftCashiers(int shift_id)
        {
            using (var _db = new Tranzit_OSEntities())
            {
                var list = _db.ShiftCashiers.Where(w => w.SHIFTID == shift_id).Select(s => new
                {
                    s.ID,
                    s.Cashiers.NAME,
                    s.Cashiers.IS_ACTIVE
                }).ToList();

                return Ok(list);
            }
        }


        [ApiTokenAuthorize]
        [HttpPost, Route("shift/list")]
        public IHttpActionResult GetShifts(FilterWb req)
        {
            string startStr = req.start_date.ToString("yyyyMMddHHmmss");
            string endStr = req.end_date.ToString("yyyyMMddHHmmss");

            using (var _db = new Tranzit_OSEntities())
            {
                var list = _db.Shifts
                    .Where(w => w.SAREAID == context_ka.KaId
                             && string.Compare(w.WORKDAYSTART, startStr) >= 0
                             && string.Compare(w.WORKDAYSTART, endStr) <= 0) 
                    .ToList()
                    .Select(s => new {
                        s.ID,
                        WORKDAYSTART = DateTime.ParseExact(s.WORKDAYSTART, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture)
                    })
                    .ToList();

                return Ok(list);
            }
        }


        [ApiTokenAuthorize]
        [HttpPost, Route("open-shift")]
        public IHttpActionResult OpenShift(List<int> cashiers_ids)
        {
            // 1. Валідація вхідних даних (захист від порожніх запитів)
            if (cashiers_ids == null || !cashiers_ids.Any())
            {
                return BadRequest("Список касирів не може бути порожнім.");
            }

            using (var _db = new Tranzit_OSEntities())
            {
                var newShift = new Shifts
                {
                    SAREAID = context_ka.KaId,
                    WORKDAYSTART = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ShiftCashiers = new List<ShiftCashiers>()
                };

                foreach (var cashierId in cashiers_ids)
                {
                    newShift.ShiftCashiers.Add(new ShiftCashiers
                    {
                        CASHIERID = cashierId
                    });
                }
                _db.Shifts.Add(newShift);

                _db.SaveChanges();

                return Ok(true);
            }
        }


        [ApiTokenAuthorize]
        [HttpGet, Route("cashiers/list")]
        public IHttpActionResult GetCashiers()
        {
            using (var _db = new Tranzit_OSEntities() )
            {

                return Ok(_db.Cashiers.Where(w=> w.SAREAID == context_ka.KaId && w.IS_ACTIVE == true).Select(s => new { s.ID, s.NAME }).ToList());

            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("cashiers")]
        public IHttpActionResult AddCashier(Cashiers c)
        {
            using (var _db = new Tranzit_OSEntities())
            {
                c.SAREAID = context_ka.KaId;
                c.IS_ACTIVE = true;

                _db.Cashiers.Add(c);
                _db.SaveChanges();

                return Ok(true);
            }
        }

        [ApiTokenAuthorize]
        [HttpPost, Route("{id}/cashiers/del")]
        public IHttpActionResult DelCashier(int id)
        {
            using (var _db = new Tranzit_OSEntities())
            {
                var c = _db.Cashiers.Find(id);
                if (c != null)
                {
                    _db.Cashiers.Remove(c);
                    _db.SaveChanges();
                }

                return Ok(true);
            }
        }




    }
}