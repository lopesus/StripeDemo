using System;
using System.Linq;
using System.Threading.Tasks;

namespace StripeDemo.Models.PaymentsGateway
{
    public class FuturePaymentIntentModel
    {
        public string Id { get; set; }
        public string IntentSecret { get; set; }
        public CustomerModel Customer { get; set; }
    }
}
