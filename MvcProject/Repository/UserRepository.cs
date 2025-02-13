using Dapper;
using Microsoft.AspNetCore.Http;
using MvcProject.Exceptions;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Data;
namespace MvcProject.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly IDbConnection _connection;

        public UserRepository(ILogger<UserRepository> logger, IDbConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }
        public async Task<string> GenerateTokens(string userId)
        {
            var publicToken = Guid.NewGuid().ToString();
            var privateToken = Guid.NewGuid().ToString();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@publicToken", publicToken);
            parameters.Add("@publicIsValid", 1);
            parameters.Add("@privateToken", privateToken);
            parameters.Add("@privateIsValid", 1);
            parameters.Add("@OutputPublicToken", dbType: DbType.String, size: 40, direction: ParameterDirection.Output);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _connection.ExecuteAsync(
                "RegisterTokens", parameters, commandType: CommandType.StoredProcedure);
            var returnCode = parameters.Get<int>("@ReturnCode");
            var outputPublicToken = parameters.Get<string>("@OutputPublicToken");
            if (returnCode != 0)
            {
                CustomStatusCode status = Enum.IsDefined(typeof(CustomStatusCode), returnCode)
                                                          ? (CustomStatusCode)returnCode
                                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(status, $"Something's Wrong. StatusCode: {returnCode} ({status})");
            }
            return outputPublicToken;
        }

    }
}
