using Application.Chats.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors
{
    public static class ChatErrorHandler
    {
        public static ObjectResult ToObjectResult(this ChatException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ChatNotFoundException => StatusCodes.Status404NotFound,
                    ChatMessageNotFoundException => StatusCodes.Status404NotFound,
                    ChatMemberNotFoundException => StatusCodes.Status404NotFound,
                    UserNotMemberException => StatusCodes.Status403Forbidden,
                    UserAlreadyMemberException => StatusCodes.Status400BadRequest,
                    InsufficientPermissionsException => StatusCodes.Status403Forbidden,
                    ChatOperationFailedException => StatusCodes.Status500InternalServerError,
                    ChatMessageOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ChatErrorHandler.")
                }
            };
        }

        public static ActionResult ToActionResult(this ChatException exception)
        {
            return exception.ToObjectResult();
        }
    }
}
