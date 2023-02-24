using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;

namespace WebApi.Core
{
    public class HttpRequestContext
    {
        #region Construction

        public HttpRequestContext(HttpContextBase context)
        {
            _context = context;
        }

        public HttpRequestContext(HttpRequestMessage request) : this(GetContextFromRequest(request))
        {
        }

        public HttpRequestContext(HttpRequest request) : this(request.RequestContext.HttpContext)
        {
        }

        public HttpRequestContext(HttpContext context) : this(new HttpContextWrapper(context))
        {
        }

        #endregion

        #region Public Methods

        public string GetHeader(string name)
        {
            var headers = _context.Request.Headers;
            return headers[name];
        }

        public HttpRequestBase GetHttpRequestBase()
        {
            return _context.Request;
        }

        #endregion

        #region Private Methods

        protected static HttpContextWrapper GetContextFromRequest(HttpRequestMessage request)
        {
            // try fast metchod
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return (HttpContextWrapper)request.Properties["MS_HttpContext"];
            }

            // use thread lock
            return new HttpContextWrapper(HttpContext.Current);
        }

        #endregion

        #region Fields

        protected readonly HttpContextBase _context;
        public HttpContextBase Context
        {
            get { return _context; }
        }

        #endregion

        #region Methods

        public string GetClientIp()
        {
            // check ARR
            var result = _context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains(":"))
                {
                    return result.Split(':')[0];
                }

                return result;
            }


            // direct call
            return _context.Request.UserHostAddress;
        }

        public string GetHost()
        {
            // check ARR
            var result = _context.Request.ServerVariables["HTTP_X_ORIGINAL_HOST"];

            // no proxy
            if (string.IsNullOrEmpty(result))
            {
                result = _context.Request.Url.Host;
            }

            // remove default port
            var uri = new Uri((IsSecureConnection() ? "https://" : "http://") + result);

            return uri.Host;
        }

        public string GetHttpsOn()
        {
            // check ARR
            var result = _context.Request.ServerVariables["HTTP_X_ORIGINAL_HTTPS"];

            // no proxy
            if (string.IsNullOrEmpty(result))
            {
                result = _context.Request.ServerVariables["HTTPS"];
            }

            return result;
        }

        public bool IsSecureConnection()
        {
            return string.Equals(GetHttpsOn(), "ON", StringComparison.OrdinalIgnoreCase);
        }

        public string GetRequestBody()
        {
            string result = null;

            if (_context.Request.InputStream != null)
            {
                using (var stream = new MemoryStream())
                {
                    _context.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    _context.Request.InputStream.CopyTo(stream);
                    result = Encoding.UTF8.GetString(stream.ToArray());
                }
            }

            return result;
        }

        public NameValueCollection QueryString
        {
            get { return _context.Request.QueryString; }
        }

        public Uri GetOriginalUrl()
        {
            // check ARR
            var result = _context.Request.ServerVariables["HTTP_X_ORIGINAL_CACHE_URL"];

            // no proxy
            //if (string.IsNullOrEmpty(result))
            //{
            //    result = _context.Request.ServerVariables["CACHE_URL"];
            //}


            if (string.IsNullOrEmpty(result))
            {
                result = _context.Request.Url.AbsoluteUri;
            }

            return new Uri(result);

            //return "80" == result ? "" : result;
        }

        public string GetBaseUrl(string virtual_path_root)
        {
            var url = GetOriginalUrl();
            var host = url.GetLeftPart(UriPartial.Authority);
            var base_href = host + virtual_path_root;
            return base_href;
        }

        #endregion
    }
}