using MvcProject.Models.Model;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<string> GenerateTokens(string userId);
    }
}
