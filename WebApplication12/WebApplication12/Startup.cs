using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebApplication12.Startup))]
namespace WebApplication12
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
