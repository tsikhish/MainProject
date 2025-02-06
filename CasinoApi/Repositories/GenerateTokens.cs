using CasinoApi.Models;
using CasinoApi.Repositories.IRepositories;
using Dapper;
using System.Data;

namespace CasinoApi.Repositories
{
    public class GenerateTokens : IGenerateTokens
    {
        private readonly IDbConnection _connection;

        public GenerateTokens(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<Token> GeneratePrivateTokens(Guid publicToken)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PublicToken", publicToken);
            parameters.Add("@PrivateToken", dbType: DbType.String, size: 40, direction: ParameterDirection.Output);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _connection.ExecuteAsync(
                "GetPrivateToken", parameters, commandType: CommandType.StoredProcedure);
            var returnCode = parameters.Get<int>("@ReturnCode");
            var privateToken = parameters.Get<string>("@PrivateToken");
            return new Token { PrivateToken = privateToken, StatusCode = returnCode };
        }
    }
}
