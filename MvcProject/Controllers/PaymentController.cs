using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Service;
using MvcProject.Service.IService;

namespace MvcProject.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ITransactionService _transactionService;

        public PaymentController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
       
        [Authorize]
        [Route("Payment/{DepositWithdrawId}/{Amount}")]
        public async Task<IActionResult> Index(int depositWithdrawId, int amount)
        {
            var transaction = await _transactionService.GetDepositWithdraw(depositWithdrawId, amount);
            return View("Index", transaction);
        }
        public async Task<IActionResult> SendingDeposit(DepositWithdrawRequest depositWithdrawRequest)
        {
            var response =await _transactionService.SendingDepositToBankingApi(depositWithdrawRequest);
            return View(response);
        }
    }
}
