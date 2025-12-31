using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerEncashment
{
    public class RouteView
    {
        public long Uid { get; set; }
        public long RouteListId { get; set; }
        public string Num { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Name { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
       
    }
}