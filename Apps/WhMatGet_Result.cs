﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Apps
{
    using System;

    public partial class WhMatGet_Result
    {
        public Nullable<long> RecNo { get; set; }
        public int MatId { get; set; }
        public Nullable<decimal> Remain { get; set; }
        public Nullable<decimal> Rsv { get; set; }
        public string MatName { get; set; }
        public string MsrName { get; set; }
        public string Artikul { get; set; }
        public Nullable<decimal> AvgPrice { get; set; }
        public string GrpName { get; set; }
        public Nullable<int> Num { get; set; }
        public string BarCode { get; set; }
        public string Country { get; set; }
        public string Producer { get; set; }
        public Nullable<decimal> MinReserv { get; set; }
        public Nullable<decimal> Ordered { get; set; }
        public Nullable<decimal> ORsv { get; set; }
        public Nullable<int> IsSerial { get; set; }
        public Nullable<int> OutGrpId { get; set; }
        public Nullable<decimal> CurRemain { get; set; }
        public Nullable<decimal> SumRemain { get; set; }
    }
}