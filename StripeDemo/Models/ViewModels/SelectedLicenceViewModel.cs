using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class SelectedLicenceViewModel
{
    public ErrorCodes ErrorCodes { get; set; }
    public UserInfos UserInfos { get; set; }

    public ProductModel ProductModel { get; set; }
}