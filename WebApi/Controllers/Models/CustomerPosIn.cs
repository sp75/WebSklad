using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Controllers.Models
{
    public class CustomerPosIn
    {
        public int PosId { get; set; }
        public int WbillId { get; set; }
        public string DocNum { get; set; }
        public DateTime OnDate { get; set; }
        public DateTime DocDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal BasePrice { get; set; }
        public decimal CurRemain { get; set; }
        public decimal Remain { get; set; }
        public decimal Rsv { get; set; }
        public int? KaId { get; set; }
        public int? PosParent { get; set; }
        public decimal TotalRemain { get; set; }
    }
}