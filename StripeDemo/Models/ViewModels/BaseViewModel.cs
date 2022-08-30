using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class BaseViewModel
{
    public ErrorCodes ErrorCodes { get; set; }
    public UserInfos UserInfos { get; set; }
}