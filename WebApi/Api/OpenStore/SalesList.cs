using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.OpenStore
{
    public class SalesList
    {
        public int ARTID { get; set; }
        public int ARTCODE { get; set; }
        public string ARTNAME { get; set; }
        public int SESSID { get; set; }
        public int SAREAID { get; set; }
        public int SYSTEMID { get; set; }
        public DateTime SessionStartDate { get; set; }
        public int MatId { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }
}