using Azure.Core;
using CasinoApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
namespace CasinoApi.Controllers
{
    [ApiController]
    [Route("[controller]")] 
    public class AuthenticateController : Controller
    {
        private readonly IDbConnection _connection;
        public AuthenticateController(IDbConnection connection)
        {
            _connection=connection;
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePrivateToken([FromBody] Token request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("Invalid request data.");
            }
            try
            {
                await SaveTokensToDatabase(request);
                return Ok(new
                {
                    PrivateToken = request.PrivateToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task SaveTokensToDatabase(Token request)
        {
            var query = "Exec InseretToken @UserId,@PublicToken,@PublicIsValid,@PrivateToken,@PrivateIsValid";
            await _connection.ExecuteAsync(query, new
            {
                UserId = request.UserId,
                PrivateToken = request.PrivateToken,
                PublicToken = request.PublicToken,
                PublicIsValid = false,
                PrivateIsValid = true
            });
        }
    }
}
