using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class AccountViewModel: BaseViewModel
{
    public bool HasNewPaymentAccount { get; set; }
}