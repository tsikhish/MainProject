using MvcProject.Models.Model;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IWithdrawRepository
    {
        public Task<Response> GetResponse(Response response);
        public Task<Withdraw> GetWithdrawTransaction(int id);
        Task<Response> SendWithdrawToBankingApi(Withdraw withdraw);
    }
}
