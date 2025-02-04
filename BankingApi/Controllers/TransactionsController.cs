﻿using BankingApi.Helper;
using BankingApi.Models;
using BankingApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
namespace BankingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : Controller
    {
        private readonly string _secretKey;
        private readonly string _merchantId;
        private readonly string _apiUrl;
        private readonly ISendBackResponse _sendBackResponse;
        private readonly IHash _hash;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            IHash hash,
            IOptions<AppSettings> appSettings,
            ISendBackResponse sendBackResponse,
            ILogger<TransactionsController> logger)
        {
            _hash = hash;
            _sendBackResponse = sendBackResponse;
            _secretKey = appSettings.Value.SecretKey;
            _merchantId = appSettings.Value.MerchantID;
            _apiUrl = appSettings.Value.ApiUrl;
            _logger = logger;
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] Deposit deposit)
        {
            try
            {
                _logger.LogInformation("Processing Deposit for TransactionID: {TransactionID}, Amount: {Amount}", deposit.TransactionID, deposit.Amount);

                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount), _merchantId, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                {
                    _logger.LogWarning("Hash mismatch for Deposit TransactionID: {TransactionID}", deposit.TransactionID);
                    return BadRequest("Incorrect hash");
                }

                string paymentUrl = $"{_apiUrl}/{deposit.TransactionID}/{(int)(deposit.Amount)}";
                _logger.LogInformation("Deposit processed successfully. Payment URL: {PaymentUrl}", paymentUrl);

                return Ok(new { Status = 1, PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Deposit for TransactionID: {TransactionID}", deposit.TransactionID);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ConfirmDeposit")]
        public async Task<IActionResult> ConfirmDeposit([FromBody] Deposit deposit)
        {
            try
            {
                _logger.LogInformation("Confirming Deposit for TransactionID: {TransactionID}, Amount: {Amount}", deposit.TransactionID, deposit.Amount);

                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount), _merchantId, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                {
                    _logger.LogWarning("Hash mismatch for ConfirmDeposit TransactionID: {TransactionID}", deposit.TransactionID);
                    return BadRequest("Incorrect hash");
                }

                bool isAmountEven = (deposit.Amount / 100) % 2 == 0;
                var status = isAmountEven ? "Success" : "Rejected";
                _logger.LogInformation("Deposit confirmed with status: {Status} for TransactionID: {TransactionID}", status, deposit.TransactionID);

                return Ok(new
                {
                    TransactionId = deposit.TransactionID,
                    Status = status,
                    Amount = deposit.Amount,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming Deposit for TransactionID: {TransactionID}", deposit.TransactionID);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ConfirmWithdraw")]
        public async Task<IActionResult> ConfirmWithdraw([FromBody] Withdraw withdraw)
        {
            try
            {
                _logger.LogInformation("Confirming Withdraw for TransactionID: {TransactionID}, Amount: {Amount}", withdraw.TransactionID, withdraw.Amount);

                var hash = _hash.ComputeSHA256Hash((int)(withdraw.Amount), _merchantId, withdraw.TransactionID, withdraw.UsersFullName, _secretKey);
                if (hash != withdraw.Hash)
                {
                    _logger.LogWarning("Hash mismatch for ConfirmWithdraw TransactionID: {TransactionID}", withdraw.TransactionID);
                    return BadRequest("Incorrect hash");
                }

                bool isAmountEven = (withdraw.Amount / 100) % 2 == 0;
                var result = isAmountEven ? Status.Success : Status.Rejected;
                _logger.LogInformation("Withdraw result: {Result} for TransactionID: {TransactionID}", result, withdraw.TransactionID);

                await _sendBackResponse.SendWithdrawResultToMvcProject(withdraw, result);
                return Ok(new
                {
                    DepositWithdrawRequestId = withdraw.TransactionID,
                    Status = result,
                    Amount = withdraw.Amount,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming Withdraw for TransactionID: {TransactionID}", withdraw.TransactionID);
                return BadRequest(ex.Message);
            }
        }
    }
}