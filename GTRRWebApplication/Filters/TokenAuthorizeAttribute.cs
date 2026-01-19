using GTRR_DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace GTRRWebApplication.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!IsTokenValid(context, out string? message, out HttpStatusCode statusCode))
            {
                context.Result = new JsonResult(new
                {
                    Message = message
                })
                {
                    StatusCode = (int)statusCode
                };
                return;
            }

            await Task.CompletedTask;
        }


        private bool IsTokenValid(AuthorizationFilterContext context, out string? message, out HttpStatusCode statusCode)
        {
            message = null;
            statusCode = HttpStatusCode.Unauthorized;

            var request = context.HttpContext.Request;

            // UserId
            if (!request.Headers.TryGetValue("UserId", out var userIdValues))
            {
                message = "No UserId header found";
                return false;
            }

            if (!int.TryParse(userIdValues.FirstOrDefault(), out int userId))
            {
                message = "UserId header is not a valid integer";
                return false;
            }

            // Authorization
            if (!request.Headers.TryGetValue("Authorization", out var authValues))
            {
                message = "No Authorization header found";
                return false;
            }

            var authHeader = authValues.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                message = "Invalid or missing Bearer token";
                return false;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            return YourTokenValidationLogic(token, userId, out message, out statusCode);
        }


        private bool YourTokenValidationLogic(string token, int userId, out string? message, out HttpStatusCode statusCode)
        {
            message = null;
            statusCode = HttpStatusCode.Unauthorized;

            try
            {
                bool isValid = GTRR_HelperDAL.ValidateUserToken(
                    userId,
                    token,
                    out bool isTokenValid,
                    out string? dbMessage
                );


                if (!string.IsNullOrEmpty(dbMessage) &&
                    dbMessage.Equals("Inactive Application", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Inactive Application";
                    statusCode = HttpStatusCode.Forbidden;
                    return false;
                }
                if (!string.IsNullOrEmpty(dbMessage) &&
                  dbMessage.Equals("Password Expired", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Your password has expired. Please reset your password to continue";
                    statusCode = HttpStatusCode.Forbidden;
                    return false;
                }


                if (!isValid || !isTokenValid)
                {
                    message = "Token is invalid or expired.";
                    statusCode = HttpStatusCode.Unauthorized;
                    return false;
                }

                return true;
            }
            catch
            {
                message = "Error during token validation.";
                statusCode = HttpStatusCode.InternalServerError;
                return false;
            }
        }

    }
}
