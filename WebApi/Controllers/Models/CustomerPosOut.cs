using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Controllers.Models
{
    public class CustomerPosOut
    {
        public int PosId { get; set; }
        public int WbillId { get; set; }
        public string Num { get; set; }
        public DateTime OnDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal? ReturnAmount { get; set; }
        public decimal Remain { get; set; }
    }
}