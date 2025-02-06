using CasinoApi.Models;
using CasinoApi.Repositories.IRepositories;
using Dapper;
using log4net;
using System.Data;
using System.Net;

namespace CasinoApi.Repositories
{
    public class CreatingGames : ICreatingGames
    {
        private readonly IDbConnection _connection;
        private readonly ILog _logger;
        public CreatingGames(ILog Logger,IDbConnection connection)
        {
            _logger = Logger;
            _connection = connection;
        }

        public async Task<Response> CreateBet(BetRequest betRequest)
        {
            try
            {
                _logger.Info($"Creating bet for transaction ID: {betRequest.TransactionId}");
                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", betRequest.PrivateToken);
                parameters.Add("@Amount", betRequest.Amount);
                parameters.Add("@TransactionId", betRequest.TransactionId);
                parameters.Add("@BetTypeId", betRequest.BetTypeId);
                parameters.Add("@GameId", betRequest.GameId);
                parameters.Add("@ProductId", betRequest.ProductId);
                parameters.Add("@RoundId", betRequest.RoundId);
                parameters.Add("@Hash", betRequest.Hash);
                parameters.Add("@Currency", betRequest.Currency);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                await _connection.ExecuteAsync("CreateBet", parameters, commandType: CommandType.StoredProcedure);

                var statusCode = parameters.Get<int>("@ReturnCode");
                var currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                if (statusCode == 200)
                    _logger.Info($"Bet Request was successfully got for Transaction ID: {betRequest.TransactionId}");
                else _logger.Warn($"Bet Request was not got for Transaction ID: {betRequest.TransactionId}");

                return new Response
                {
                    CurrentBalance = currentBalance,
                    TransactionId = betRequest.TransactionId,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while processing bet", ex);
                throw new Exception("Error while processing bet: " + ex.Message, ex);
            }
        }
        public async Task<Response> CreateWin(WinRequest winRequest)
        {
            try
            {
                _logger.Info($"Creating win for transaction ID: {winRequest.TransactionId}");

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", winRequest.PrivateToken);
                parameters.Add("@Amount", winRequest.Amount);
                parameters.Add("@TransactionId", winRequest.TransactionId);
                parameters.Add("@WinTypeId", winRequest.WinTypeId);
                parameters.Add("@GameId", winRequest.GameId);
                parameters.Add("@ProductId", winRequest.ProductId);
                parameters.Add("@RoundId", winRequest.RoundId);
                parameters.Add("@Hash", winRequest.Hash);
                parameters.Add("@Currency", winRequest.Currency);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                await _connection.ExecuteAsync("CreateWin", parameters, commandType: CommandType.StoredProcedure);

                var statusCode = parameters.Get<int>("@ReturnCode");
                var currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                if (statusCode == 200)
                    _logger.Info($"Win was successfully got for Transaction ID: {winRequest.TransactionId}");
                else _logger.Warn($"Win was not got for Transaction ID: {winRequest.TransactionId}");

                return new Response
                {
                    CurrentBalance = currentBalance,
                    TransactionId = winRequest.TransactionId,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while processing win", ex);

                throw new Exception("Error while processing bet: " + ex.Message, ex);
            }

        }
        public async Task<Response> CancelBet(CancelBet cancelBet)
        {
            try
            {
                _logger.Info($"Canceling Bet for transaction ID: {cancelBet.TransactionId}");

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", cancelBet.PrivateToken);
                parameters.Add("@Amount", cancelBet.Amount);
                parameters.Add("@TransactionId", cancelBet.TransactionId);
                parameters.Add("@BetTypeId", cancelBet.BetTypeId);
                parameters.Add("@GameId", cancelBet.GameId);
                parameters.Add("@ProductId", cancelBet.ProductId);
                parameters.Add("@RoundId", cancelBet.RoundId);
                parameters.Add("@Hash", cancelBet.Hash);
                parameters.Add("@Currency", cancelBet.Currency);
                parameters.Add("@BetTransactionId", cancelBet.BetTransactionId);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                await _connection.ExecuteAsync("CancelBet", parameters, commandType: CommandType.StoredProcedure);
                var statusCode = parameters.Get<int>("@ReturnCode");
                var currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                if (statusCode == 200)
                    _logger.Info($"Bet was successfully got for TransactionId ID: {cancelBet.TransactionId}");
                else _logger.Warn($"Bet was not got for TransactionId ID: {cancelBet.TransactionId}");

                return new Response
                {
                    CurrentBalance = currentBalance,
                    TransactionId = cancelBet.TransactionId,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while processing win", ex);

                throw new Exception("Error while processing bet: " + ex.Message, ex);
            }

        }
        public async Task<Response> ChangeWin(ChangeWin changeWin)
        {
            try
            {
                _logger.Info($"Changing Win for transaction ID: {changeWin.TransactionId}");

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", changeWin.PrivateToken);
                parameters.Add("@Amount", changeWin.Amount);
                parameters.Add("@PreviousAmount", changeWin.PreviousAmount);
                parameters.Add("@TransactionId", changeWin.TransactionId);
                parameters.Add("@PreviousTransactionId", changeWin.PreviousTransactionId);
                parameters.Add("@ChangeWinType", changeWin.ChangeWinType);
                parameters.Add("@GameId", changeWin.GameId);
                parameters.Add("@ProductId", changeWin.ProductId);
                parameters.Add("@RoundId", changeWin.RoundId);
                parameters.Add("@Hash", changeWin.Hash);
                parameters.Add("@Currency", changeWin.Currency);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                await _connection.ExecuteAsync("ChangeWin", parameters, commandType: CommandType.StoredProcedure);
                var statusCode = parameters.Get<int>("@ReturnCode");
                var currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                if (statusCode == 200)
                    _logger.Info($"Win was successfully changed for TransactionId ID: {changeWin.TransactionId}");
                else _logger.Warn($"Win was not changed for TransactionId ID: {changeWin.TransactionId}");

                return new Response
                {
                    CurrentBalance = currentBalance,
                    TransactionId = changeWin.TransactionId,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while processing changing win", ex);

                throw new Exception("Error while processing bet: " + ex.Message, ex);
            }

        }
        public async Task<Response> GetBalance(GetBalance getBalance)
        {
            try
            {
                _logger.Info($"Get Balance for Game ID: {getBalance.GameId}");

                var parameters = new DynamicParameters();
                parameters.Add("@PrivateToken", getBalance.PrivateToken);
                parameters.Add("@GameId", getBalance.GameId);
                parameters.Add("@ProductId", getBalance.ProductId);
                parameters.Add("@Hash", getBalance.Hash);
                parameters.Add("@Currency", getBalance.Currency);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@CurrentBalance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                await _connection.ExecuteAsync("GetBalance", parameters, commandType: CommandType.StoredProcedure);
                var statusCode = parameters.Get<int>("@ReturnCode");
                var currentBalance = parameters.Get<decimal?>("@CurrentBalance") ?? 0;
                if (statusCode ==200)
                    _logger.Info($"Balance was successfully got for GameId ID: {getBalance.GameId}");
                else _logger.Warn($"Balance was not got for GameId ID: {getBalance.GameId}");
                return new Response
                {
                    CurrentBalance = currentBalance,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Error while processing getBalance", ex);

                throw new Exception("Error while processing bet: " + ex.Message, ex);
            }

        }

    }
}
