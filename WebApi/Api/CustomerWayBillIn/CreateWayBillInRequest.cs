using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerWayBillIn
{
    public class CreateWayBillInRequest
    {
        public string Num { get; set; }
        public DateTime OnDate { get; set; }

        public string Notes { get; set; }
        public int KaId { get; set; }
        public bool CreatePayment { get; set; }

        public List<WayBillInItem> Items { get; set; }
    }
}