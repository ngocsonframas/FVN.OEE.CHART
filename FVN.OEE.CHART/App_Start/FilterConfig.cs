using System.Web;
using System.Web.Mvc;

namespace FVN.OEE.CHART
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
