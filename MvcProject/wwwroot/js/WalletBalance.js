function updateWalletBalance() {
    $.ajax({
        url: '/Wallet/GetWalletBalance',
        type: 'GET',
        success: function (response) {
            if (response && response.balance !== undefined) {
                $('#walletBalance').text('Balance: ' + response.currency + response.balance.toFixed(2));
            } else {
                $('#walletBalance').text('Balance: Error loading');
            }
        },
        error: function () {
            $('#walletBalance').text('Balance: Error loading');
        }
    });
}

$(document).ready(function () {
    updateWalletBalance();
    setInterval(updateWalletBalance, 30000);
});
