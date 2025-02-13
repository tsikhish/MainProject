using Dapper;
using Microsoft.EntityFrameworkCore;
using MvcProject.Exceptions;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Data;
namespace MvcProject.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly IDbConnection _dbConnection;
        public WalletRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task CreateWalletByUserIdAsync(string userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dbConnection.ExecuteAsync("CreateWallet", parameters, commandType: CommandType.StoredProcedure);

            var returnCode = parameters.Get<int>("@ReturnCode");

            if (returnCode != 0)
            {
                CustomStatusCode status = Enum.IsDefined(typeof(CustomStatusCode), returnCode)
                                                          ? (CustomStatusCode)returnCode
                                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(status, $"Something's Wrong. StatusCode: {returnCode} ({status})");
            }
        }

        public async Task<int> GetWalletCurrencyByUserIdAsync(string userId)
        {
            var query = "Select Currency from Wallet where UserId=@userId";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId });
        }

        public async Task<decimal> GetWalletBalanceByUserIdAsync(string userId)
        {
            var query = "Select CurrentBalance from Wallet where UserId=@userId";
            return await _dbConnection.QueryFirstOrDefaultAsync<decimal>(query, new { UserId = userId });
        }
    }

}
