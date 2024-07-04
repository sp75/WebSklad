using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerInventory
{
    public class InventoryActDet
    {
        public Guid Id { get; set; }
        public int? Num { get; set; }
        public Guid InventoryActId { get; set; }
        public int ARTID { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}