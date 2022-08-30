// Set your publishable key: remember to change this to your live publishable key in production
// See your keys here: https://dashboard.stripe.com/apikeys
const stripe = Stripe('pk_test_51LVreJIU31sah1iglF3AWPrAdoDsvZeuTth48bfh77Fz51aS5Q24ZojjTWcovefoI4R3NAmOl6OjWHYNTZ2WV5vr00w6CNELbu');

const options = {
    clientSecret: '{{CLIENT_SECRET}}',
    // Fully customizable with appearance API.
    appearance: {
        theme: 'stripe',
    },
};

initialize();
checkPaymentStatus();



// Fetches a payment intent and captures the client secret
async function initialize() {
    //todo store on server and fetch for improved security ??? to check 
    let subscriptionId= window.sessionStorage.getItem('subscriptionId');
    let clientSecret = window.sessionStorage.getItem('clientSecret');

    options.clientSecret = clientSecret;

    //const response = await fetch("/Licence/CreateSubscription", {
    //    method: "POST",
    //    headers: { "Content-Type": "application/json" },
    //    body: JSON.stringify({ items }),
    //});
    //const { clientSecret2 } = await response.json();

    //const appearance = {
    //    theme: 'stripe',
    //};
    //elements = stripe.elements({ appearance, clientSecret });


    // Set up Stripe.js and Elements to use in checkout form, passing the client secret obtained in step 5
    const elements = stripe.elements(options);

    // Create and mount the Payment Element
    const paymentElement = elements.create('payment');
    paymentElement.mount('#payment-element');
    


    const form = document.getElementById('payment-form');
    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        setLoading(true);

        const { error } = await stripe.confirmPayment({
            //`Elements` instance that was used to create the Payment Element
            elements,
            confirmParams: {
                return_url: "https://localhost:7230/Licence/SubscriptionCompleted",
                //return_url: "/Licence/SubscriptionCompleted",
            }
        });

        if (error) {
            // This point will only be reached if there is an immediate error when
            // confirming the payment. Show error to your customer (for example, payment
            // details incomplete)
            const messageContainer = document.querySelector('#error-message');
            messageContainer.textContent = error.message;
        } else {
            // Your customer will be redirected to your `return_url`. For some payment
            // methods like iDEAL, your customer will be redirected to an intermediate
            // site first to authorize the payment, then redirected to the `return_url`.
        }

        setLoading(false);
    });
}

async function checkPaymentStatus() {

    // Retrieve the "payment_intent_client_secret" query parameter appended to
    // your return_url by Stripe.js
    const clientSecret = new URLSearchParams(window.location.search).get(
        'payment_intent_client_secret'
    );

    // Retrieve the PaymentIntent
    stripe.retrievePaymentIntent(clientSecret).then(({ paymentIntent }) => {
        const message = document.querySelector('#error-message')

        // Inspect the PaymentIntent `status` to indicate the status of the payment
        // to your customer.
        //
        // Some payment methods will [immediately succeed or fail][0] upon
        // confirmation, while others will first enter a `processing` state.
        //
        // [0]: https://stripe.com/docs/payments/payment-methods#payment-notification
        switch (paymentIntent.status) {
        case 'succeeded':
            message.innerText = 'Success! Payment received.';
            break;

        case 'processing':
            message.innerText = "Payment processing. We'll update you when payment is received.";
            break;

        case 'requires_payment_method':
            message.innerText = 'Payment failed. Please try another payment method.';
            // Redirect your user back to your payment page to attempt collecting
            // payment again
            break;

        default:
            message.innerText = 'Something went wrong.';
            break;
        }
    });
}

// Show a spinner on payment submission
function setLoading(isLoading) {
    if (isLoading) {
        // Disable the button and show a spinner
        document.querySelector("#submit").disabled = true;
        document.querySelector("#spinner").classList.remove("hidden");
        document.querySelector("#button-text").classList.add("hidden");
    } else {
        document.querySelector("#submit").disabled = false;
        document.querySelector("#spinner").classList.add("hidden");
        document.querySelector("#button-text").classList.remove("hidden");
    }
}


