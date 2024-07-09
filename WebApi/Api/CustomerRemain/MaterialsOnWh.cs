using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerRemain
{
    public class MaterialsOnWh
    {
        public int MatId { get; set; }
        public string MatName { get; set; }
        public string MsrName { get; set; }
        public decimal Remain { get; set; }
        public decimal Rsv { get; set; }
        public decimal CurRemain { get; set; }
        public string Artikul { get; set; }
        public decimal? AvgPrice { get; set; }
        public string GrpName { get; set; }
        public decimal? SumRemain { get; set; }
        public int? OpenStoreId { get; set; }
        public int? TypeId { get; set; }
        public decimal? AmountSold { get; set; }

    }
}