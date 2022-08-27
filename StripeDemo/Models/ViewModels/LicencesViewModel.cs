using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class LicencesViewModel
{
    public ErrorCodes ErrorCodes { get; set; }
    public UserInfos UserInfos { get; set; }

    public List<ProductModel> ProductModelList { get; set; }
}