using CasinoApi.Models;
using CasinoApi.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : Controller
    {
        private readonly ICreatingGames _creatingGames;

        public GamesController(ICreatingGames creatingGames)
        {
            _creatingGames = creatingGames;
        }
        [HttpPost("CreateBet")]
        public async Task<IActionResult> CreateBet(BetRequest betrequest)
        {
            try
            {
                var response = await _creatingGames.CreateBet(betrequest);
                if (response.StatusCode == 200 || response.StatusCode == 201) return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        TransactionId = response.TransactionId,
                        CurrentBalance = response.CurrentBalance,
                    }
                });
                return BadRequest(new { StatusCode = response.StatusCode });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("CreateWin")]
        public async Task<IActionResult> CreateWin(WinRequest winRequest)
        {
            try
            {
                var response = await _creatingGames.CreateWin(winRequest);
                if (response.StatusCode == 200 || response.StatusCode == 201) return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        TransactionId = response.TransactionId,
                        CurrentBalance = response.CurrentBalance,
                    }
                });
                return BadRequest(new { StatusCode = response.StatusCode });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("CancelBet")]
        public async Task<IActionResult> CancelBet(CancelBet cancelBet)
        {
            try
            {
                var response = await _creatingGames.CancelBet(cancelBet);
                if (response.StatusCode == 200 || response.StatusCode == 201) return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        TransactionId = response.TransactionId,
                        CurrentBalance = response.CurrentBalance,
                    }
                });
                return BadRequest(new { StatusCode = response.StatusCode });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("ChangeWin")]
        public async Task<IActionResult> ChangeWin(ChangeWin changeWin)
        {
            try
            {
                var response = await _creatingGames.ChangeWin(changeWin);
                if (response.StatusCode == 200 || response.StatusCode == 201) return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        TransactionId = response.TransactionId,
                        CurrentBalance = response.CurrentBalance,
                    }
                });
                return BadRequest(new { StatusCode = response.StatusCode });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("GetBalance")]
        public async Task<IActionResult> GetBalance(GetBalance getBalance)
        {
            try
            {
                var response = await _creatingGames.GetBalance(getBalance);
                if (response.StatusCode == 200 || response.StatusCode == 201) return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        TransactionId = response.TransactionId,
                        CurrentBalance = response.CurrentBalance,
                    }
                });
                return BadRequest(new { StatusCode = response.StatusCode });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
}
}
