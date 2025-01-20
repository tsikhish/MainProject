using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.Model;
using Newtonsoft.Json;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        public CallbackController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
        public IActionResult Payment()
        {
            var transactionJson = TempData["Transaction"] as string;
            if (transactionJson == null)
            {
                return BadRequest("No transaction data found.");
            }

            var transaction = JsonConvert.DeserializeObject<Deposit>(transactionJson);
            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit(Deposit deposit)
        {
            deposit.Amount = (decimal)(deposit.Amount / 100);
            await _transactionRepository.RegisterTransactionInTransactionsAsync(deposit);
            await _transactionRepository.UpdateWalletAmount(deposit);
            await _transactionRepository.UpdateDepositTable(deposit);
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
