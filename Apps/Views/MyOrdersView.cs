using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSklad.Apps.Views
{
    public class MyOrdersView
    {
        public Guid Id { get; set; }
        public int WbillId { get; set; }
        public string PersonName { get; set; }
        public string Num { get; set; }
        public DateTime OnDate { get; set; }
        public string Reason { get; set; }
        public int Checked { get; set; }
        public decimal? SummAll { get; set; }
        public int WType { get; set; }
        public int? PersonId { get; set; }
        public decimal? SummPay { get; set; }
        public string Notes { get; set; }
        public decimal? SummInCurr { get; set; }
        public string CurrName { get; set; }
        public int? KaId { get; set; }
        public decimal? Balans { get; set; }
    }
}