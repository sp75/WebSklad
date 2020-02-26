using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSklad.Models
{
    public static class Excel
    {
        public static void RespondExcel(byte[] content, string file_name)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition",
                string.Format("inline; filename={0}", file_name));
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.OutputStream.Write(content, 0, content.Length);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
    }
}
