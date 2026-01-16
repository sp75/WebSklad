using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerWriteOf
{
    public class WriteOfItem
    {
        public int Num { get; set; }
        public string MatName { get; set; }
        public string Artikul { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public int MatId { get; set; }
    }
}