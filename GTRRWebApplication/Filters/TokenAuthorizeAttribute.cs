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
            string? message;

            if (!IsTokenValid(context, out message))
            {
                context.Result = new JsonResult(new
                {
                    status = (int)HttpStatusCode.Unauthorized,
                    message = message
                })
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return;
            }

            await Task.CompletedTask;
        }

        private bool IsTokenValid(AuthorizationFilterContext context, out string? message)
        {
            message = null;

            var request = context.HttpContext.Request;

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

            if (!request.Headers.TryGetValue("Authorization", out var authValues))
            {
                message = "No Authorization header found";
                return false;
            }

            var authHeader = authValues.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                message = "Invalid or missing Bearer token";
                return false;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            return YourTokenValidationLogic(token, userId, out message);
        }

        private bool YourTokenValidationLogic(string token, int userId, out string? message)
        {
            message = null;

            try
            {
                bool isValid = GTRR_HelperDAL.ValidateUserToken(userId, token, out bool isTokenValid);

                if (!isValid)
                {
                    message = "Token is invalid or expired.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "Error during token validation: " + ex.Message;
                return false;
            }
        }
    }
}
