using Stripe;

namespace StripeDemo.Models.PaymentsGateway
{
    public class StripePaymentsGateway : IPaymentsGateway
    {
        private readonly ILogger<StripePaymentsGateway> logger;

        public StripePaymentsGateway(ILogger<StripePaymentsGateway> logger, string apiKey)
        {
            this.logger = logger;
            StripeConfiguration.ApiKey = apiKey;
        }


        public async Task<CustomerModel> GetCustomerByEmail(string email, params PaymentModelInclude[] includes)
        {
            var service = new CustomerService();
            var stripeCustomers = await service.ListAsync(new CustomerListOptions()
            {
                Email = email
            });

            if (!stripeCustomers.Any())
                return null;

            var stripeCustomer = stripeCustomers.FirstOrDefault();

            if (stripeCustomer != null)
            {
                var customerModel = new CustomerModel(stripeCustomer.Id)
                {
                    Email = email,
                    Name = stripeCustomer.Name
                };
                if (includes.Any() && includes.Contains(PaymentModelInclude.PaymentMethods))
                {
                    var paymentMethods = await GetPaymentMethods(stripeCustomer.Id, PaymentMethodType.Card);
                    customerModel.PaymentMethods = paymentMethods;
                }

                return customerModel;
            }

            return null;
        }


        public async Task<List<ProductModel>> GetProductSubscriptions()
        {
            List<ProductModel> resultList = new List<ProductModel>();
            //var productListOptions = new ProductListOptions
            //{
            //    Active = true,
            //};
            //var productService = new ProductService();
            //StripeList<Product> products = await productService.ListAsync(productListOptions);//.List(productListOptions);

            //var priceListOptions = new PriceListOptions
            //{
            //    Active = true
            //};
            ////retourne aussi les infos sur le produit
            //priceListOptions.AddExpand("data.product");
            //var priceService = new PriceService();
            //StripeList<Price> prices = priceService.List(priceListOptions);

            var options = new PriceSearchOptions
            {
                Query = "active:'true' AND metadata['category']:'subscription'",
            };
            //retourne aussi les infos sur le produit
            options.AddExpand("data.product");
            var service = new PriceService();
            var result = await service.SearchAsync(options);
            var priceList = result.Data;
            Console.WriteLine(result);


            foreach (Price price in priceList)
            {
                var product = price.Product;

                ProductModel productModel = new ProductModel();
                productModel.Id = product.Id;
                productModel.Name = product.Name;
                productModel.Description = product.Description;
                productModel.PriceModel = new ProductPriceModel()
                {
                    Id = price.Id,
                    Currency = price.Currency == "usd" ? Currency.USD : Currency.Eur,
                    Interval = price.Recurring?.Interval == "month" ? PriceInterval.Monthly : PriceInterval.Yearly,
                    UnitAmount = price.UnitAmount.GetValueOrDefault()
                };

                resultList.Add(productModel);
            }

            return resultList;
        }

        public async Task<Customer> CreateCustomer(string name, string email, string systemId)
        {
            logger.LogInformation("Creating Customer in Stripe");
            try
            {
                var options = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = new Dictionary<string, string>()
                    {
                        { "ID", systemId}
                    }
                };
                var service = new CustomerService();
                Customer customer = await service.CreateAsync(options);
                logger.LogInformation("Customer Created succesfully");
                return customer;
            }
            catch (Exception ex)
            {
                logger.LogInformation($"An error occured during customer creation, {ex}");
                return null;
            }
        }


        public async Task<List<ProductModel>> PopulateProducts(List<ProductModel> products)
        {
            var productService = new ProductService();
            var existingProducts = await productService.ListAsync(new ProductListOptions()
            {
                Active = true
            });

            var priceService = new PriceService();
            var existingPrices = await priceService.ListAsync(new PriceListOptions()
            {
                Active = true
            });

            List<ProductModel> result = new List<ProductModel>();

            foreach (var product in products)
            {
                var existingProduct = existingProducts.FirstOrDefault(x => x.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase));
                IEnumerable<Price> prices;
                if (existingProduct != null)
                {
                    logger.LogInformation($"Product with NAME:{product.Name} already exists.");
                    var existingPricesForProduct = existingPrices.Where(x => x.ProductId == existingProduct.Id);
                    prices = await CreatePrices(product.Prices, existingProduct, existingPricesForProduct);
                }
                else
                {
                    var options = new ProductCreateOptions
                    {
                        Name = product.Name,
                    };

                    logger.LogInformation($"Creating Product with NAME:{product.Name}");
                    existingProduct = await productService.CreateAsync(options);
                    logger.LogInformation("Product created succesfully");
                    prices = await CreatePrices(product.Prices, existingProduct);
                }

                result.Add(new ProductModel()
                {
                    Id = existingProduct.Id,
                    Name = existingProduct.Name,
                    Prices = prices.Select(p => new ProductPriceModel()
                    {
                        Id = p.Id,
                        Currency = p.Currency == "usd" ? Currency.USD : Currency.Eur,
                        Interval = p.Recurring?.Interval == "month" ? PriceInterval.Monthly : PriceInterval.Yearly,
                        UnitAmount = p.UnitAmount.GetValueOrDefault()
                    }).ToList()
                });
            }
            return result;
        }

        private async Task<IEnumerable<Price>> CreatePrices(List<ProductPriceModel> prices, Product existingProduct, IEnumerable<Price> existingPricesForProduct = null)
        {
            List<Price> stripePrices = new List<Price>();
            var priceService = new PriceService();
            foreach (var price in prices)
            {
                var existingPrice = existingPricesForProduct?.FirstOrDefault(x => x.UnitAmount == price.UnitAmount);
                if (existingPrice != null)
                {
                    logger.LogInformation($"Price with AMOUNT:{existingPrice.UnitAmount} for Product with NAME:{existingProduct.Name} already exists.");
                    continue;
                }

                var options = new PriceCreateOptions
                {
                    Product = existingProduct.Id,
                    UnitAmount = price.UnitAmount,
                    Currency = price.Currency.ToString().ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = price.Interval == PriceInterval.Monthly ? "month" : "year"
                    },
                };
                var createdPrice = await priceService.CreateAsync(options);
                stripePrices.Add(createdPrice);
            }
            return stripePrices;
        }


        public async Task<PaymentMethodModel> AttachPaymentMethod(string paymentMethodId, string customerId, bool makeDefault = true)
        {
            try
            {
                var options = new PaymentMethodAttachOptions
                {
                    Customer = customerId,
                };
                var service = new PaymentMethodService();
                var stripePaymentMethod = await service.AttachAsync(paymentMethodId, options);

                if (makeDefault)
                {
                    // Update customer's default invoice payment method
                    var customerOptions = new CustomerUpdateOptions
                    {
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = stripePaymentMethod.Id,
                        },
                    };
                    var customerService = new CustomerService();
                    await customerService.UpdateAsync(customerId, customerOptions);
                }

                PaymentMethodModel result = new PaymentMethodModel(stripePaymentMethod.Id);

                if (!Enum.TryParse(stripePaymentMethod.Type, true, out PaymentMethodType paymentMethodType))
                {
                    logger.LogError($"Cannot recognize PAYMENT_METHOD_TYPE:{stripePaymentMethod.Type}");
                }
                result.Type = paymentMethodType;

                if (result.Type == PaymentMethodType.Card)
                {
                    result.Card = new PaymentMethodCardModel()
                    {
                        Brand = stripePaymentMethod.Card.Brand,
                        Country = stripePaymentMethod.Card.Country,
                        ExpMonth = stripePaymentMethod.Card.ExpMonth,
                        ExpYear = stripePaymentMethod.Card.ExpYear,
                        Issuer = stripePaymentMethod.Card.Issuer,
                        Last4 = stripePaymentMethod.Card.Last4,
                        Description = stripePaymentMethod.Card.Description,
                        Fingerprint = stripePaymentMethod.Card.Fingerprint,
                        Funding = stripePaymentMethod.Card.Funding,
                        Iin = stripePaymentMethod.Card.Iin
                    };
                }

                return result;
            }
            catch (StripeException se)
            {
                logger.LogError($"An error occured during attach of PAYMENT_METHOD:{paymentMethodId} for CUSTOMER:{customerId}, {se}");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occured during attach of PAYMENT_METHOD:{paymentMethodId} for CUSTOMER:{customerId}, {ex}");
            }
            return null;
        }

        public async Task DeletePaymentMethod(string paymentMethodId)
        {
            var service = new PaymentMethodService();
            var paymentMethod = await service.DetachAsync(paymentMethodId);

        }

        public async Task<bool> CreateSubscription(string customerEmail, string priceId)
        {
            var stripeCustomer = await GetCustomerByEmail(customerEmail);
            if (stripeCustomer == null)
                return false;

            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = stripeCustomer.Id,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = await subscriptionService.CreateAsync(subscriptionOptions);
                return true;
            }
            catch (StripeException e)
            {
                logger.LogError($"An error occured during creation of subscription for CUSTOMER:{stripeCustomer.Id} and PRICE:{priceId}, {e}");
                return false;
            }
        }

        public async Task<FuturePaymentIntentModel> PrepareForFuturePaymentWithCustomerEmail(string customerEmail)
        {
            var stripeCustomer = await GetCustomerByEmail(customerEmail);
            if (stripeCustomer == null)
                return null;

            FuturePaymentIntentModel intent = await PrepareForFuturePayment(stripeCustomer.Id);
            return intent;
        }

        public async Task<FuturePaymentIntentModel> PrepareForFuturePayment(string customerId)
        {
            var options = new SetupIntentCreateOptions
            {
                Customer = customerId,
                Expand = new List<string>()
                {
                    "customer"
                }
            };

            var service = new SetupIntentService();
            var intent = await service.CreateAsync(options);
            return new FuturePaymentIntentModel()
            {
                Id = intent.Id,
                IntentSecret = intent.ClientSecret,
                Customer = new CustomerModel(intent.Customer.Id)
                {
                    Email = intent.Customer.Email,
                    Name = intent.Customer.Name,
                    SystemId = intent.Customer.Metadata?.GetValueOrDefault("ID"),
                }
            };
        }


        public async Task<List<PaymentMethodModel>> GetPaymentMethods(string customerId, PaymentMethodType paymentMethodType)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = paymentMethodType.ToString().ToLower()
            };

            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(options);


            List<PaymentMethodModel> result = new List<PaymentMethodModel>();
            foreach (var stripePaymentMethod in paymentMethods)
            {
                if (!Enum.TryParse(stripePaymentMethod.Type, true, out PaymentMethodType currPaymentMethodType))
                {
                    logger.LogError($"Cannot recognize PAYMENT_METHOD_TYPE:{stripePaymentMethod.Type}");
                    continue;
                }

                PaymentMethodModel currentPaymentMethod = new PaymentMethodModel(stripePaymentMethod.Id)
                {
                    Type = currPaymentMethodType
                };

                if (currPaymentMethodType == PaymentMethodType.Card)
                {
                    currentPaymentMethod.Card = new PaymentMethodCardModel()
                    {
                        Brand = stripePaymentMethod.Card.Brand,
                        Country = stripePaymentMethod.Card.Country,
                        ExpMonth = stripePaymentMethod.Card.ExpMonth,
                        ExpYear = stripePaymentMethod.Card.ExpYear,
                        Issuer = stripePaymentMethod.Card.Issuer,
                        Last4 = stripePaymentMethod.Card.Last4,
                        Description = stripePaymentMethod.Card.Description,
                        Fingerprint = stripePaymentMethod.Card.Fingerprint,
                        Funding = stripePaymentMethod.Card.Funding,
                        Iin = stripePaymentMethod.Card.Iin
                    };
                }

                result.Add(currentPaymentMethod);
            }
            return result;
        }

        public async Task<List<PaymentMethodModel>> GetPaymentMethodsByCustomerEmail(string customerEmail, PaymentMethodType paymentMethodType)
        {
            CustomerModel customer = await GetCustomerByEmail(customerEmail);

            return await GetPaymentMethods(customer.Id, paymentMethodType);
        }


        public async Task ChargeWithCustomerEmail(string customerEmail, string paymentMethodId, Currency currency, long unitAmount,
            bool sendEmailAfterSuccess = false, string emailDescription = "")
        {
            var customer = await GetCustomerByEmail(customerEmail);
            await Charge(customer.Id, paymentMethodId, currency, unitAmount, customerEmail, sendEmailAfterSuccess, emailDescription);
        }

        // customize receipt -> https://dashboard.stripe.com/settings/branding
        // -> https://dashboard.stripe.com/settings/billing/invoice
        // in case of email send uppon failure -> https://dashboard.stripe.com/settings/billing/automatic
        public async Task Charge(string customerId, string paymentMethodId,
            Currency currency, long unitAmount, string customerEmail, bool sendEmailAfterSuccess = false, string emailDescription = "")
        {
            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentCreateOptions
                {
                    Amount = unitAmount,
                    Currency = currency.ToString().ToLower(),
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    OffSession = true,
                    ReceiptEmail = sendEmailAfterSuccess ? customerEmail : null,
                    Description = emailDescription,
                };
                await service.CreateAsync(options);
            }
            catch (StripeException e)
            {
                switch (e.StripeError.Type)
                {
                    case "card_error":
                        // Error code will be authentication_required if authentication is needed
                        Console.WriteLine("Error code: " + e.StripeError.Code);
                        var paymentIntentId = e.StripeError.PaymentIntent.Id;
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(paymentIntentId);

                        Console.WriteLine(paymentIntent.Id);
                        break;
                    default:
                        break;
                }
            }
        }


        public async Task<IEnumerable<ChargeModel>> GetPaymentStatus(string paymentId)
        {
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentId);
            var charges = intent.Charges.Data;

            return charges.Select(x => new ChargeModel(x.Id)
            {
                Status = x.Status
            });
        }
    }
}
