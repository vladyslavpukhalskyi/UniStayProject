using Application.Users.Exceptions; // Розташування ваших UserException
using Microsoft.AspNetCore.Http;    // Для StatusCodes
using Microsoft.AspNetCore.Mvc;     // Для ObjectResult
using System;                        // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class UserErrorHandler
    {
        public static ObjectResult ToObjectResult(this UserException exception)
        {
            // За замовчуванням, тіло відповіді буде містити повідомлення з винятку.
            // Ви можете розширити це, щоб створити більш структурований об'єкт помилки,
            // наприклад, використовуючи ProblemDetails або власний DTO помилки.
            // var errorResponse = new { message = exception.Message, userId = exception.UserId.ToString() };
            // return new ObjectResult(errorResponse)

            // Дотримуємося прикладу ActorErrorHandler: повертаємо сам exception.Message
            return new ObjectResult(new { Message = exception.Message }) // Обгортаємо повідомлення в об'єкт для кращого JSON
            {
                StatusCode = exception switch
                {
                    UserNotFoundException => StatusCodes.Status404NotFound,
                    UserAlreadyExistsException => StatusCodes.Status409Conflict,
                    UserOperationFailedException => StatusCodes.Status500InternalServerError,
                    // Додайте інші ваші специфічні підтипи UserException тут
                    // _ => StatusCodes.Status500InternalServerError // Загальна помилка для інших UserException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in UserErrorHandler.")
                }
            };
        }
    }
}