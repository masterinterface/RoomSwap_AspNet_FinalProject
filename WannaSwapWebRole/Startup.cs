using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WannaSwapWebRole.Startup))]
namespace WannaSwapWebRole
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
