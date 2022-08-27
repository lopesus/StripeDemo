using Microsoft.AspNetCore.Mvc;
using StripeDemo.Models;
using System.Diagnostics;
using StripeDemo.Models.ViewModels;

namespace StripeDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CheckOut()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            var body = Request.Body;
            var query = Request.Query;
            var queryString = Request.QueryString;
            return View();
        }
        public IActionResult PaymentError()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}