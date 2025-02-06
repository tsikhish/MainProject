using CasinoApi.Models;

namespace CasinoApi.Repositories.IRepositories
{
    public interface IGenerateTokens
    {
        Task<Token> GeneratePrivateTokens(Guid publicToken);
    }
}
