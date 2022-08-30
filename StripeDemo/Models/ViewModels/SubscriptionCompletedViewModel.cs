using StripeDemo.Models.PaymentsGateway;

namespace StripeDemo.Models.ViewModels;

public class SubscriptionCompletedViewModel: BaseViewModel
{
    public string RequestBody { get; set; }

    //for debug
    public string PaymentIntent { get; set; }
    public string PaymentIntentClientSecret { get; set; }
    public string QueryString { get; set; }
}