using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KnowDetroit.Startup))]
namespace KnowDetroit
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
