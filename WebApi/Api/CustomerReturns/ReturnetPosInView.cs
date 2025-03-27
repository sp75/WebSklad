using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerReturns
{
    public class ReturnetPosInView
    {
        public int Id { get; set; }
        public int PosId { get; set; }
        public decimal Amount { get; set; }
        public decimal? BasePrice { get; set; }
        public int? KaId { get; set; }
        public int WId { get; set; }
        public int MatId { get; set; }
        public decimal? Price { get; set; }
        public decimal? CurRemain { get; set; }
    }
}