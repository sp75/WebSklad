using SP.Base;
using System;
using System.Net.Http;
using System.Web;
using System.Linq;

namespace WebApi.Core
{
    public class ApiTokenContext : HttpRequestContext
    {
        
        public Guid? Token { get; private set; }

        public ApiTokenContext(HttpContextBase context)
            : base(context)
        {
            var token = GetRequestToken();
            if (token.HasValue)
            {
                using (var sp_base = Database.SPBase())
                {
                    if (sp_base.Kagent.Any(w => w.Id == token.Value))
                    {
                        Token = token;
                    }
                }

            }
        }

        public ApiTokenContext(HttpRequestMessage request) 
            : this( GetContextFromRequest( request ) )
        {
        }

        private Guid? GetRequestToken()
        {
            var token_string = GetHeader("X-Api-Token");

            Guid token;
            if (Guid.TryParse(token_string, out token))
            {
                return token;
            }

            return null;
        }


    }
}