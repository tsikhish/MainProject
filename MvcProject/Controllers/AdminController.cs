using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Repository.IRepository;
using MvcProject.Service;

namespace MvcProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILog _logger;
        private readonly IBankingRequestService _bankingRequestService;

        public AdminController(ILog logger,IBankingRequestService bankingRequestService, IWithdrawRepository withdrawRepository,
            ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
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

        [HttpPost]
        public async Task<IActionResult> RejectWithdraw(int id)
        {
            await _transactionRepository.UpdateRejectedStatus(id);
            TempData["RejectMessage"] = "The withdrawal has been rejected.";
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptWithdraw(int id)
        {
            var transaction = await _withdrawRepository.GetWithdrawTransaction(id);
            var response = await _bankingRequestService.SendWithdrawToBankingApi(transaction);
            return View(response);
        }
    }
}
