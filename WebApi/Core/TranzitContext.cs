using System;
using System.Net.Http;
using System.Web;

namespace WebApi.Core
{
    public class TranzitContext : HttpRequestContext
    {
        
        public Guid? Token { get; private set; }

        public TranzitContext(HttpContextBase context)
            : base(context)
        {
            var token = GetRequestToken();
            if (token.HasValue)
            {
                var kay = token.Value ==  new Guid("E9ECD622-7F68-44C3-9075-00009304BD35");
              //  CurrentDriver = driver_repo.FindByToken(token.Value);
                if (kay)
                {
                    Token = token;
                }
            }
        }

        public TranzitContext(HttpRequestMessage request) 
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