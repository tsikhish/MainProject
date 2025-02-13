using log4net;
using MvcProject.Exceptions;
using MvcProject.Models.Enum;
using System.Net;

namespace MvcProject.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = LogManager.GetLogger(typeof(ExceptionHandlingMiddleware));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CustomException ex)
            {
                _logger.Error("An error occurred while processing the request.", ex);

                context.Response.StatusCode = ex.StatusCode switch
                {
                    CustomStatusCode.AlreadyExists => (int)HttpStatusCode.Conflict,
                    CustomStatusCode.PendingRequestExists => (int)HttpStatusCode.BadRequest,
                    CustomStatusCode.InsufficientBalance => (int)HttpStatusCode.PaymentRequired,
                    CustomStatusCode.AmountMismatch => (int)HttpStatusCode.BadRequest,
                    CustomStatusCode.NotExists => (int)HttpStatusCode.NotFound,
                    CustomStatusCode.ChangedStatus => (int)HttpStatusCode.Conflict,
                    _ => (int)HttpStatusCode.InternalServerError
                };


                context.Response.ContentType = "application/json";
                var response = new { message = ex.Message, statusCode = ex.StatusCode };
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while processing the request.", ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsJsonAsync(new { message = "Internal Server Error" });
            }
        }
    }

}

