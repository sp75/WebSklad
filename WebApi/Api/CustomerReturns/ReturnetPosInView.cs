using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerReturns
{
    public class ReturnetPosInView
    {
        public int RemoteId { get; set; }
        public int MatId { get; set; }
        public decimal? Price { get; set; }
        public decimal ReturnedAmount { get; set; }
    }
}