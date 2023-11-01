using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using WebApi.Controllers;

namespace WebApi.Core
{
    public class ApiTokenAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext action_context)
        {
            var controller = action_context.ControllerContext.Controller as BaseApiController;
            var token = controller != null && controller.Context != null ? controller.Context.Token : null;
            return token != null;
        }
    }
}