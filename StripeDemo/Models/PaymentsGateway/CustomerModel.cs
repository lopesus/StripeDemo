using System.Collections.Generic;

namespace StripeDemo.Models.PaymentsGateway
{
    public class CustomerModel
    {
        public string Id { get; set; }
        public string SystemId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public List<PaymentMethodModel> PaymentMethods { get; set; }

        public CustomerModel(string id)
        {
            Id = id;
        }
    }
}