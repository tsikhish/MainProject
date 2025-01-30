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
        private readonly ILogger<AdminController> _logger;
        private readonly IBankingRequestService _bankingRequestService;
        public AdminController(ILogger<AdminController> logger,IBankingRequestService bankingRequestService,IWithdrawRepository withdrawRepository,
            IDepositRepository depositRepository, ITransactionRepository transactionRepository)
        {
            _logger = logger;
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
                _logger.LogInformation("Fetching withdrawal transactions for the admin dashboard.");

                var withdraws = await _transactionRepository.GetWithdrawTransactionsForAdmins();
                if (withdraws == null || !withdraws.Any())
                {
                    _logger.LogWarning("No withdrawal transactions found for the admin dashboard.");
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved {Count} withdrawal transactions for the admin dashboard.", withdraws.Count());
                }

                return View(withdraws);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching withdrawal transactions for the admin dashboard.");
                return StatusCode(500, "An error occurred while retrieving data.");
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
