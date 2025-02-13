using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.DTO;

namespace MvcProject.Service.IService
{
    public interface IWithdrawService
    {
        Task RegisterWithdraw(string userId, [FromBody] DepositRequestDTO request);
        Task SuccessWithdraw([FromBody] Response response);
        Task<Response> AcceptWithdraw(int id);

    }
}
