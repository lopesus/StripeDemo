using Microsoft.AspNetCore.Mvc;

namespace StripeDemo.Tools
{
    public class Utils
    {
        public static string GetControllerName<T>() where T : Controller
        {
            return typeof(T).Name.Replace(nameof(Controller), string.Empty);
        }
    }
}
