using GTRRWebApplication.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GTRRWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet("crash")]
        public IActionResult Crash()
        {
            try
            {
                // example crash
                int x = 0;
                var y = 10 / x;
                return Ok(y);
            }
            catch (Exception ex)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    // Add request info
                    scope.SetTag("http.method", Request.Method);
                    scope.SetTag("url", $"{Request.Scheme}://{Request.Host}{Request.Path}");
                    scope.SetExtra("query_string", Request.QueryString.ToString());
                    scope.SetExtra("user_id", Request.Headers["UserId"].ToString());

                    // Optionally add custom message
                    scope.SetExtra("custom_message", "Crash in /crash endpoint");

                    // Capture exception
                    SentrySdk.CaptureException(ex);
                });

                return StatusCode(500, "Captured by Sentry");
            }
        }

    }
}
