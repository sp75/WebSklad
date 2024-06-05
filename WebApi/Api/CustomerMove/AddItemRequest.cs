using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerMove
{
    public class AddItemRequest
    {
        public int ArtId { get; set; }
        public int WbillId { get; set; }
        public decimal Amount { get; set; }
    }
}