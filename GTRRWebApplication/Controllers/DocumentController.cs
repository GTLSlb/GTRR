using GTRR_DataAccessLayer;
using GTRRWebApplication.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GTRRWebApplication.Controllers
{
    [Route("api/GTRR/V1")]
    [ApiController]
    [TokenAuthorize]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ILogger<DocumentController> logger)
        {
            _logger = logger;
        }



        [HttpGet("RequiredDocs")]
        [GzipCompression]
        public async Task<IActionResult> GetRequiredDocs()
        {
            try {
                if (!Request.Headers.TryGetValue("UserId", out var headerValue) ||
                    !int.TryParse(headerValue.FirstOrDefault(), out int userId))
                {

                    var message = "Invalid or missing UserId header parameter";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                }

                _logger.LogInformation(
                    "\nMethod: {Method}" +
                    "\nRequest: {Url}" +
                    "\nHeader:\nUserId={UserId}\n",
                    Request.Method,
                    $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
                    Request.Headers["UserId"].ToString()
                    );


                var (json, msg, error) = await GTRR_HelperDAL.GetRequiredDocs(userId);

                if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
                {
                    _logger.LogInformation(
                         "\nMethod: {Method}" +
                         "\nRequest: {Url}" +
                         "\nHeader:\nUserId={UserId}\nError: {Error}\nMessage: {Msg}",
                         Request.Method,
                         $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
                         Request.Headers["UserId"].ToString(),
                         error,
                         msg
                     );

                    SentrySdk.CaptureMessage($"GetRequiredDocs failed. Error: {error} | Msg: {msg}", SentryLevel.Error);

                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                    return BadRequest(new { message = responseMessage });
                }




                var data = JsonSerializer.Deserialize<object>(json);
                _logger.LogInformation(
                       "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                       "GetRequiredDocs",
                       "200 OK",
                       new string('-', 200)
                    );
               

                return Ok(data);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in GetRequiredDocs");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }



        [HttpPost("RequiredDocs")]
        public async Task<IActionResult> AddEditRequiredDocuments([FromBody] object vehicletype)
        {
            try { 
            if (!Request.Headers.TryGetValue("UserId", out var headerValues) ||
                !int.TryParse(headerValues.FirstOrDefault(), out int userId))
            {
                    var message = "Invalid or missing UserId header parameter";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                }

            if (vehicletype == null)
            {
                    var message = "Invalid or missing request body";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                 
            }

            _logger.LogInformation(
                    "\nRequest: [{Method}] {Path}" +
                    "\nUserId: {UserId}" +
                    "\nBody: {Body}" +
                    "\n{Separator}",
                    Request.Method,
                    Request.Path,
                    userId,
                    JsonSerializer.Serialize(vehicletype),
                    new string('-', 200)
                    );



            var (msg, error) = await GTRR_HelperDAL.AddEditRequiredDocuments(userId, JsonSerializer.Serialize(vehicletype));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogInformation(
                     "\nMethod: {Method}" +
                     "\nRequest: {Url}" +
                     "\nHeader:\nUserId={UserId}\nError: {Error}\nMessage: {Msg}",
                     Request.Method,
                     $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
                     Request.Headers["UserId"].ToString(),
                     error,
                     msg
                 );
                    SentrySdk.CaptureMessage($"AddEditRequiredDocuments failed. Error: {error} | Msg: {msg}", SentryLevel.Error);
                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage });
            }

            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "GetSenderReceivers",
                    "200 OK",
                    new string('-', 200)
                );

              


                return Ok();
        }
              catch (Exception ex)
            {
 
                _logger.LogError(ex, "Unhandled exception in AddEditRequiredDocuments");
                SentrySdk.CaptureException(ex);
 
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

    }
}
