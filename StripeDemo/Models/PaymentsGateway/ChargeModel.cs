namespace StripeDemo.Models.PaymentsGateway
{
    public class ChargeModel
    {
        public string Id { get; }
        public string Status { get; set; }

        public ChargeModel(string id)
        {
            Id = id;
        }
    }
}