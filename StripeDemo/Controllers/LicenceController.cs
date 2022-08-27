using Microsoft.AspNetCore.Mvc;
using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Models.ViewModels;
using StripeDemo.Services;

namespace StripeDemo.Controllers
{
    public class LicenceController : Controller
    {
        private IUserInfosService userInfosService;
        private IPaymentsGateway paymentsGateway;
        public LicenceController(IUserInfosService userInfosService, IPaymentsGateway paymentsGateway)
        {
            this.userInfosService = userInfosService;
            this.paymentsGateway = paymentsGateway;
        }
        public async Task<IActionResult> Index()
        {
            var vm = new LicencesViewModel();
            var user = userInfosService.GetCurrentUser();
            if (user != null)
            {
                vm.UserInfos = user;
            }
            else
            {
                vm.ErrorCodes = ErrorCodes.UserNotFound;
            }
            var products = await paymentsGateway.GetProductSubscriptions();
            if (products != null)
            {
                vm.ProductModelList = products;
            }
            else
            {
                vm.ErrorCodes = ErrorCodes.PaymentGatewayError;
            }

            return View(vm);
        }

        public async Task<IActionResult> Select(string id)
        {
            var vm = new SelectedLicenceViewModel();
            var user = userInfosService.GetCurrentUser();
            if (user != null)
            {
                vm.UserInfos = user;
            }
            else
            {
                vm.ErrorCodes = ErrorCodes.UserNotFound;
            }
            var products = await paymentsGateway.GetProductSubscriptions();
            var selectedProduct = products.FirstOrDefault(p => p.Id == id);
            if (selectedProduct != null)
            {
                vm.ProductModel = selectedProduct;
            }
            else
            {
                vm.ErrorCodes = ErrorCodes.PaymentGatewayError;
            }

            return View(vm);
        }
    }
}
