using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.Manufacture
{
    public class ManufactureProductRequest
    {
        public int RecId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}