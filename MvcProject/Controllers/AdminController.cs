using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using MvcProject.Models.Service;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class AdminController : Controller
    {
        public readonly IWithdrawRepository _withdrawRepository;
        public readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBankingRequestService _bankingRequestService;
        public AdminController(IBankingRequestService bankingRequestService,IWithdrawRepository withdrawRepository,
            IDepositRepository depositRepository, ITransactionRepository transactionRepository)
        {
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _depositRepository= depositRepository;
            _transactionRepository = transactionRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var withdraws = await _transactionRepository.GetWithdrawTransactionsForAdmins();
                return View(withdraws);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> RejectWithdraw(int id)
        {
            try
            {
                await _transactionRepository.UpdateRejectedStatus(id);
                TempData["RejectMessage"] = "The withdrawal has been rejected.";
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AcceptWithdraw(int id)
        {
            try
            {
                var transaction = await _withdrawRepository.GetWithdrawTransaction(id);
                var response = await _bankingRequestService.SendWithdrawToBankingApi(transaction);
                return View(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
