using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class AccountViewModel
{
    public ErrorCodes ErrorCodes { get; set; }
    public UserInfos UserInfos { get; set; }
    public bool HasNewPaymentAccount { get; set; }
}