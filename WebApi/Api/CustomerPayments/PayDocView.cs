using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerPayments
{
    public class PayDocView
    {
        public string ChargeName { get; set; }

        public string PayTypeName { get; set; }
        public string SourceType { get; set; }
        public string PersonName { get; set; }

        public int PayDocId { get; set; }
        public DateTime OnDate { get; set; }
        public decimal Total { get; set; }
        public string DocNum { get; set; }
        public int Checked { get; set; }
        public string Reason { get; set; }

        public string Notes { get; set; }
    }
}