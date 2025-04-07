using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerReturns
{
    public class GetPosOutView
    {
        public int PosId { get; set; }
        public DateTime OnDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? Nds { get; set; }
        public int? CurrId { get; set; }
        public decimal? OnValue { get; set; }
        public decimal? Discount { get; set; }
        public decimal Remain { get; set; }
        public decimal RemoteAmount { get; set; }
        public int? WId { get; set; }
        public int MatId { get; set; }
        public int RemoteId { get; set; }
        public int? WbillIdOut { get; set; }
        public decimal ReturnedAmount { get; set; }
    }
}