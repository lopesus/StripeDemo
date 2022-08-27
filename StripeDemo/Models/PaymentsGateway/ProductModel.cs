using System.Collections.Generic;

namespace StripeDemo.Models.PaymentsGateway
{
    public class ProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductPriceModel PriceModel { get; set; }


        public List<ProductPriceModel> Prices { get; set; }
    }
}