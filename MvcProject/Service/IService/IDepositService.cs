using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.DTO;

namespace MvcProject.Service.IService
{
    public interface IDepositService
    {
        Task SuccessDeposit([FromBody] Response response);
        Task<Response> Deposit(string userId, [FromBody] DepositRequestDTO request);
    }
}
