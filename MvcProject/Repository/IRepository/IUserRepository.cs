namespace MvcProject.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<string> GenerateTokens(string userId);
    }
}
