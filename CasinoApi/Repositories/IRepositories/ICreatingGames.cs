using CasinoApi.Models;

namespace CasinoApi.Repositories.IRepositories
{
    public interface ICreatingGames
    {
        Task<Response> CreateBet(BetRequest betRequest);
        Task<Response> CreateWin(WinRequest winRequest);
        Task<Response> CancelBet(CancelBet cancelBet);
        Task<Response> ChangeWin(ChangeWin changeWin);
        Task<Response> GetBalance(GetBalance getBalance);


    }
}
