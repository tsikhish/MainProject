using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using MvcProject.Service.IService;
namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly IDepositService _depositService;
        private readonly IWithdrawService _withdrawService;
        public CallbackController(IDepositService depositService,
            IWithdrawService withdrawService)
        {
            _depositService = depositService;
            _withdrawService = withdrawService;
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit([FromBody] Response response)
        {
           await _depositService.SuccessDeposit(response);
           return Ok("Response was successfull");
        }

        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody] Response response)
        {
            await _withdrawService.SuccessWithdraw(response);
            return View(response);
        }
    }
}