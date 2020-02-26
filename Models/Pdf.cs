using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public static class Pdf
    {
        public static void RespondPdf(byte[] content, string file_name)
        {
            string type = "inline";

            var httpResponse = HttpContext.Current.Response;
            httpResponse.AddHeader("Content-Type", "application/pdf");
            httpResponse.AddHeader("Content-Disposition", string.Format(type + "; filename={1}.pdf; size={0}", content.Length, file_name));
            httpResponse.BinaryWrite(content);
            httpResponse.End();


          /*  HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition",
                string.Format("inline; filename={0}", file_name));
            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.OutputStream.Write(content, 0, content.Length);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();*/
        }
    }
}