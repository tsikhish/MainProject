using log4net;

namespace MvcProject.Models.Exceptions
{
    public class CustomExceptions : ICustomExceptions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CustomExceptions));

        public async Task WithdrawExceptions(int outputParam2Value,string userId)
        {
            if (outputParam2Value == 400)
            {
                string errorMessage = $"User {userId} has already sent a pending withdraw request.";
                _logger.WarnFormat(errorMessage);
                throw new WithdrawException(errorMessage, outputParam2Value);
            }
            else if (outputParam2Value == 401)
            {
                string errorMessage = $"User {userId} does not have enough balance for withdraw.";
                _logger.WarnFormat(errorMessage);
                throw new WithdrawException(errorMessage, outputParam2Value);
            }
            else if (outputParam2Value == 500)
            {
                string errorMessage = $"Transaction failed for user {userId}.";
                _logger.ErrorFormat(errorMessage);
                throw new WithdrawException(errorMessage, outputParam2Value);
            }
        }
        public async Task TransactionWithdrawException(int returnCode,string userId)
        {
            if (returnCode == 400)
            {
                string errorMessage = $"Amount mismatch for user {userId} on withdraw transaction.";
                _logger.WarnFormat(errorMessage);
                throw new WithdrawException("The requested amount does not match the original request.", returnCode);
            }
            if (returnCode == 401)
            {
                string errorMessage = $"Status of withdraw request already changed for user {userId}.";
                _logger.WarnFormat(errorMessage);
                throw new WithdrawException("The status of this withdrawal request has already been changed.", returnCode);
            }
            if (returnCode == 500)
            {
                string errorMessage = $"Internal error while processing withdraw transaction for user {userId}.";
                _logger.ErrorFormat(errorMessage);
                throw new WithdrawException("An internal error occurred while processing the transaction.", returnCode);
            }
        }
        public async Task DepositException(int depositId,string userId, int outputParam2Value)
        {
            if (depositId == 0)
            {
                _logger.Error($"Failed to retrieve DepositWithdrawId for user {userId}.");
                throw new Exception("Failed to retrieve DepositWithdrawId.");
            }
            if (outputParam2Value == 400)
            {
                _logger.Warn($"User {userId} already has a pending request.");
                throw new Exception("This user has already sent a pending request. Please wait for results.");
            }
            else if (outputParam2Value == 401)
            {
                _logger.Warn($"User {userId} has insufficient balance.");
                throw new Exception("Insufficient Balance");
            }
            else if (outputParam2Value == 402)
            {
                _logger.Error($"Failed to update BlockedAmount for user {userId}.");
                throw new Exception("BlockedAmount update failed");
            }
            else if (outputParam2Value == 500)
            {
                _logger.Error($"Transaction failed for user {userId}.");
                throw new Exception("Transaction Failed.");
            }

        }
    }
}
