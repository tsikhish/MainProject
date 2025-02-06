using Azure.Core;
using CasinoApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using CasinoApi.Repositories.IRepositories;
namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")] 
    public class AuthenticateController : Controller
    {
        private readonly IGenerateTokens _tokens;
        public AuthenticateController(IGenerateTokens tokens)
        {
            _tokens = tokens;
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePrivateToken(Guid publicToken)
        {
            try
            {
                var response = await _tokens.GeneratePrivateTokens(publicToken);
                if (response.StatusCode != 200)
                {
                    return BadRequest(new { StatusCode = response.StatusCode });
                }
                return Ok(new
                {
                    StatusCode = response.StatusCode,
                    Data = new
                    {
                        PrivateToken=response.PrivateToken
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
       
    }
}
