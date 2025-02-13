using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using MvcProject.Service.IService;

namespace MvcProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWithdrawService _withdrawService;
        private readonly ITransactionService _transactionService;
        public AdminController(IWithdrawService withdrawService,ITransactionService transactionService)
        {
            _withdrawService = withdrawService;
            _transactionService = transactionService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var withdraws =await _transactionService.AdminDashboard();
            return View(withdraws);
        }

        [HttpPost]
        public async Task<IActionResult> RejectWithdraw(int id)
        {
            await _transactionService.UpdateRejectedStatus(id);
            TempData["RejectMessage"] = "The withdrawal has been rejected.";
            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptWithdraw(int id)
        {
            var response =await  _withdrawService.AcceptWithdraw(id);
            return View(response);
        }
    }
}
