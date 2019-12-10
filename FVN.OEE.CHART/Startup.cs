using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FVN.OEE.CHART.Startup))]
namespace FVN.OEE.CHART
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
