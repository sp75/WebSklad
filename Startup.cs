using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebSklad.Startup))]
namespace WebSklad
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
