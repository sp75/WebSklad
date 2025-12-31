using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.OpenStore
{
    public class SalesSummaryView
    {
        public string SAREANAME { get; set; }
        public string SAREAADDR { get; set; }
        public int SAREAID { get; set; }
        public Nullable<decimal> Total { get; set; }
        public Nullable<decimal> NoFiscalSales { get; set; }
        public Nullable<decimal> FiscalSales { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string UNITNAME { get; set; }
        public string ARTNAME { get; set; }
        public int ARTCODE { get; set; }
        public int GRPID { get; set; }
        public string GRPNAME { get; set; }
        public string ARTSNAME { get; set; }
        public string AREAGRPNAME { get; set; }
    }
}