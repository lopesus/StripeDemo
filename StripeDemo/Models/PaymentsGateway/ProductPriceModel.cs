namespace StripeDemo.Models.PaymentsGateway
{
    public class ProductPriceModel
    {
        public string Id { get; set; }
        public long UnitAmount { get; set; }
        public PriceInterval Interval { get; set; }
        public Currency Currency { get; set; }
    }
}