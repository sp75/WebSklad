using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.OpenStore
{
    public class OpenStoreAreaList
    {
        public int KaId { get; set; }
        public int? OpenStoreAreaId { get; set; }
        public DateTime? LastInventoryDate { get; set; }
        public int? WId { get; set; }
    }
}