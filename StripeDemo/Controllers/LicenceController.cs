using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Stripe;
using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Models.ViewModels;
using StripeDemo.Services;

namespace StripeDemo.Controllers
{
    public class LicenceController : Controller
    {
        private IUserInfosService userInfosService;
        private IPaymentsGateway paymentsGateway;

        private StripeSettings stripeSettings;
        public LicenceController(IUserInfosService userInfosService, IPaymentsGateway paymentsGateway, IOptions<StripeSettings> options)
        {
            this.userInfosService = userInfosService;
            this.paymentsGateway = paymentsGateway;
            this.stripeSettings = options.Value;
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

        public async Task<IActionResult> Review(string id)
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

        // [HttpPost]
        public async Task<IActionResult> CollectPaymentData(string id)
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


        public ActionResult<SubscriptionCreateResponse> CreateSubscription([FromBody] CreateSubscriptionRequest req)
        {
            //var customerId = HttpContext.Request.Cookies["customer"];
            var customerId = userInfosService.GetCurrentUser().StripeCustumerId;

            // Automatically save the payment method to the subscription
            // when the first payment is successful.
            var paymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription",
            };

            // Create the subscription. Note we're expanding the Subscription's
            // latest invoice and that invoice's payment_intent
            // so we can pass it to the front end to confirm the payment
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = req.PriceId,
                    },
                },
                PaymentSettings = paymentSettings,
                PaymentBehavior = "default_incomplete",
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = subscriptionService.Create(subscriptionOptions);

                return new SubscriptionCreateResponse
                {
                    SubscriptionId = subscription.Id,
                    ClientSecret = subscription.LatestInvoice.PaymentIntent.ClientSecret,
                };
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Failed to create subscription.{e}");
                return BadRequest();
            }
        }

        public async Task<ActionResult> SubscriptionCompleted()
        {
            var vm = new SubscriptionCompletedViewModel();
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            vm.RequestBody = json;

            var body = Request.Body;
            var query = Request.Query;
            var ddd = query.TryGetValue("payment_intent", out var paymentIntent);
            var sss = query.TryGetValue("payment_intent_client_secret", out var paymentIntentClientSecret);
            var queryString = Request.QueryString;

            vm.QueryString = queryString.Value;
            vm.PaymentIntent = paymentIntent.ToString();
            vm.PaymentIntentClientSecret = paymentIntentClientSecret.ToString();
            Console.WriteLine(paymentIntent + paymentIntentClientSecret);

            var service = new PaymentIntentService();
            PaymentIntentGetOptions options=new PaymentIntentGetOptions(){};
            options.AddExpand("invoice");
            //options.AddExpand("invoiceId");
            var payment = await service.GetAsync(paymentIntent.ToString(),options);
            Console.WriteLine(payment);

            if (payment.Status== "succeeded")
            {
                Console.WriteLine();
            }
            var invoice = payment.Invoice;
            var customerId = invoice.CustomerId;
            var subscriptionId = invoice.SubscriptionId;
            //var paymentIntent = await service.GetAsync(invoice.PaymentIntentId);

            var service2 = new SubscriptionService();
            var subscription = await service2.GetAsync(subscriptionId);
            Console.WriteLine(subscription);

            var dateFinAbonnement = subscription.CurrentPeriodEnd;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> WebHook()
        {
            //todo in prod get value from environment
            var webHookSecret = stripeSettings.WebHookSecret;

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webHookSecret, 300L, false
                );
                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                return BadRequest();
            }

            if (stripeEvent.Type == "invoice.payment_succeeded")
            {
                var invoice = stripeEvent.Data.Object as Invoice;

                if (invoice.BillingReason == "subscription_create")
                {
                    // The subscription automatically activates after successful payment
                    // Set the payment method used to pay the first invoice
                    // as the default payment method for that subscription

                    // Retrieve the payment intent used to pay the subscription
                    var service = new PaymentIntentService();
                    var paymentIntent = service.Get(invoice.PaymentIntentId);

                    // Set the default payment method
                    var options = new SubscriptionUpdateOptions
                    {
                        DefaultPaymentMethod = paymentIntent.PaymentMethodId,
                    };
                    var subscriptionService = new SubscriptionService();
                    subscriptionService.Update(invoice.SubscriptionId, options);

                    Console.WriteLine($"Default payment method set for subscription: {paymentIntent.PaymentMethodId}");
                }
                Console.WriteLine($"Payment succeeded for invoice: {stripeEvent.Id}");
            }

            if (stripeEvent.Type == "invoice.paid")
            {
                var account = stripeEvent.Account;
                var invoice = stripeEvent.Data.Object as Invoice;

                // Retrieve the payment intent used to pay the subscription
                var service = new PaymentIntentService();
                if (invoice != null)
                {
                    var customerId = invoice.CustomerId;
                    var subscriptionId = invoice.SubscriptionId;
                    var paymentIntent = await service.GetAsync(invoice.PaymentIntentId);

                    var service2 = new SubscriptionService();
                    var subscription = await service2.GetAsync(subscriptionId);

                    Console.WriteLine(paymentIntent);
                }
                // Used to provision services after the trial has ended.
                // The status of the invoice will show up as paid. Store the status in your
                // database to reference when a user accesses your service to avoid hitting rate
                // limits.
            }
            if (stripeEvent.Type == "invoice.payment_failed")
            {
                // If the payment fails or the customer does not have a valid payment method,
                // an invoice.payment_failed event is sent, the subscription becomes past_due.
                // Use this webhook to notify your user that their payment has
                // failed and to retrieve new card details.
            }
            if (stripeEvent.Type == "invoice.finalized")
            {
                // If you want to manually send out invoices to your customers
                // or store them locally to reference to avoid hitting Stripe rate limits.
            }
            if (stripeEvent.Type == "customer.subscription.deleted")
            {
                // handle subscription cancelled automatically based
                // upon your subscription settings. Or if the user cancels it.
            }
            if (stripeEvent.Type == "customer.subscription.trial_will_end")
            {
                // Send notification to your user that the trial will end
            }

            return Ok();
        }
    }

    public class CreateSubscriptionRequest
    {
        public string PriceId { get; set; }
    }

    public class SubscriptionCreateResponse
    {
        public string SubscriptionId { get; set; }
        public string ClientSecret { get; set; }
    }
}
