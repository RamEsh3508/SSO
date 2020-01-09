using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AzureADTest.Startup))]
namespace AzureADTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        ConfigureAuth(app);
        }
    }
}
