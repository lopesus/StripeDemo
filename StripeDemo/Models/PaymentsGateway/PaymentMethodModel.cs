namespace StripeDemo.Models.PaymentsGateway
{
    public class PaymentMethodModel
    {
        public string Id { get; set; }
        public PaymentMethodType Type { get; set; }
        public PaymentMethodCardModel Card { get; set; }

        public PaymentMethodModel(string id)
        {
            Id = id;
        }
    }
}