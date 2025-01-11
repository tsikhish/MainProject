document.addEventListener("DOMContentLoaded", function () {
    const walletBalanceElement = document.getElementById('wallet-balance');

    if (walletBalanceElement) {
        function updateWalletBalance() {
            fetch('/api/wallet/balance')
                .then(response => {
                    if (!response.ok) throw new Error('Failed to fetch balance');
                    return response.json();
                })
                .then(data => {
                    walletBalanceElement.textContent = `Balance: ${data.balance.toFixed(2)} ${getCurrencySymbol(data.currency)}`;
                })
                .catch(error => {
                    console.error('Error fetching wallet balance:', error);
                    walletBalanceElement.textContent = 'Balance: 0.00';
                });
        }

        function getCurrencySymbol(currency) {
            switch (currency) {
                case 1: return "€"; // EUR
                case 2: return "$"; // USD
                case 3: return "₾"; // GEL
                default: return "";
            }
        }

        updateWalletBalance(); // Fetch balance on page load
        setInterval(updateWalletBalance, 30000); // Fetch every 30 seconds
    }
});
