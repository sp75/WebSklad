using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerReturns
{
    public class GetPosInView
    {
        public int PosId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal? ReturnAmount { get; set; }
        public decimal? Remain { get; set; }
    }
}