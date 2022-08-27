namespace StripeDemo.Models.PaymentsGateway
{
    public class PaymentMethodCardModel
    {
        public string Brand { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public long ExpMonth { get; set; }
        public long ExpYear { get; set; }
        public string Fingerprint { get; set; }
        public string Funding { get; set; }
        public string Iin { get; set; }
        public string Issuer { get; set; }
        public string Last4 { get; set; }
    }
}