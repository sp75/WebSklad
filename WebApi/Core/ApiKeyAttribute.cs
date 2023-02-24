using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using WebApi.Controllers;

namespace WebApi.Core
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext action_context)
        {
            var controller = action_context.ControllerContext.Controller as TranzitBaseController;
            var driver = controller != null && controller.Context != null ? controller.Context.Token : null;
            return driver != null;


            //return true;
        }
    }
}