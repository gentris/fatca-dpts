using System.Web;
using System.Web.Mvc;

namespace FATCA_Data_Preparation_and_Transmission_System
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
