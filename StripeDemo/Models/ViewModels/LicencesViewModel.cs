using StripeDemo.Models.PaymentsGateway;
using StripeDemo.Services;

namespace StripeDemo.Models.ViewModels;

public class LicencesViewModel: BaseViewModel
{
  public List<ProductModel> ProductModelList { get; set; }
}