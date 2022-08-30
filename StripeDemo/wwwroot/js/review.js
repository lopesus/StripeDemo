const createSubscription = (priceId) => {
    return fetch('/Licence/CreateSubscription', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                priceId: priceId,
            }),
        })
        .then((response) => response.json())
        .then((data) => {
            window.sessionStorage.setItem('subscriptionId', data.subscriptionId);
            window.sessionStorage.setItem('clientSecret', data.clientSecret);
            window.location.href = '/Licence/CollectPaymentData';
        })
        .catch((error) => {
            console.error('Error:', error);
        });
}