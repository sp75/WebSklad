using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Api.CustomerWriteOf
{
    public class CreateDocumentRequest
    {
        public string Num { get; set; }
        public DateTime OnDate { get; set; }

        public string Notes { get; set; }

        public List<WriteOfItem> Items { get; set; }
    }
}