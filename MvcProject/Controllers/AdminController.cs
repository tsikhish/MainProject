using log4net;
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
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdminController)); // Change to the current class
        private readonly IBankingRequestService _bankingRequestService;

        public AdminController(IBankingRequestService bankingRequestService, IWithdrawRepository withdrawRepository,
            IDepositRepository depositRepository, ITransactionRepository transactionRepository)
        {
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _depositRepository = depositRepository;
            _transactionRepository = transactionRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                _logger.Info("Fetching withdrawal transactions for the admin dashboard.");

                var withdraws = await _transactionRepository.GetWithdrawTransactionsForAdmins();
                if (withdraws == null || !withdraws.Any())
                {
                    _logger.Warn("No withdrawal transactions found for the admin dashboard.");
                }
                else
                {
                    _logger.Info("Successfully retrieved withdrawal transactions for the admin dashboard.");
                }
                return View(withdraws);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while fetching withdrawal transactions for the admin dashboard.", ex);
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
                _logger.Error(string.Format("An error occurred while rejecting the withdrawal with ID {0}.", id), ex);
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
                _logger.Error(string.Format("An error occurred while rejecting the withdrawal with ID {0}.", id), ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
