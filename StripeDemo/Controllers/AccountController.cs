using Microsoft.AspNetCore.Mvc;
using StripeDemo.Models;
using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Models.ViewModels;
using StripeDemo.Services;

namespace StripeDemo.Controllers
{
    public class AccountController : Controller
    {
        private IUserInfosService userInfosService;
        private IPaymentsGateway paymentsGateway;
        public AccountController(IUserInfosService userInfosService, IPaymentsGateway paymentsGateway)
        {
            this.userInfosService = userInfosService;
            this.paymentsGateway = paymentsGateway;
        }
        public async Task<IActionResult> Login()
        {
            var products = await paymentsGateway.GetProductSubscriptions();
            Console.WriteLine(products);

            var vm = new AccountViewModel();
            var user = userInfosService.GetCurrentUser();
            if (user != null)
            {
                var email = user.Email;
                var customer = await paymentsGateway.GetCustomerByEmail(email);

                if (customer == null)
                {
                    var stripeCustomer = await paymentsGateway.CreateCustomer(user.Name, user.Email, user.Email);
                    vm.HasNewPaymentAccount = stripeCustomer != null;
                    if (stripeCustomer != null)
                    {
                        user.StripeCustumerId = stripeCustomer.Id;
                    }
                }
                else
                {
                    user.StripeCustumerId = customer.Id;
                }
            }
            else
            {
                vm.ErrorCodes = ErrorCodes.UserNotFound;
            }

            vm.UserInfos = user;
            return View(vm);
        }
    }
}
