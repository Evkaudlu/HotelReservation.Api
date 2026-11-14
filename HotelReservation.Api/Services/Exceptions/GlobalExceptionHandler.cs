
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Services.Exceptions
{
    // Завдання 7. Глобальна обробка винятків API
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            int statusCode;
            string message;

            // Некоректні аргументи
            if (exception is ArgumentException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
            }
            // Внутрішні помилки операцій
            else if (exception is InvalidOperationException)
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Внутрішня помилка сервера";
            }
            // Інші непередбачені помилки
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = "Сталася непередбачена помилка";
            }

            // Запис помилки в консоль
            logger.LogError(
                exception,
                "Помилка під час виконання API-запиту");

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = message,
                Detail = statusCode == 400
                    ? exception.Message
                    : "Помилку зафіксовано в журналі сервера"
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                cancellationToken);

            return true;
        }
    }
}
