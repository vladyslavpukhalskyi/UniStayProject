using Application.Messages.Exceptions; // Розташування ваших MessageException
using Microsoft.AspNetCore.Http;     // Для StatusCodes
using Microsoft.AspNetCore.Mvc;      // Для ObjectResult
using System;                         // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class MessageErrorHandler
    {
        public static ObjectResult ToObjectResult(this MessageException exception)
        {
            // Тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    MessageNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо отримувач повідомлення не знайдений - це помилка даних запиту
                    ReceiverNotFoundException => StatusCodes.Status400BadRequest, 
                    
                    // Якщо користувач намагається відправити повідомлення собі - це невалідний запит
                    CannotSendMessageToSelfException => StatusCodes.Status400BadRequest, 
                    
                    // Якщо користувач не має прав на операцію з повідомленням
                    UserNotAuthorizedForMessageOperationException => StatusCodes.Status403Forbidden,
                    
                    MessageOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів MessageException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in MessageErrorHandler.")
                }
            };
        }
    }
}