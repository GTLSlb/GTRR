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
    public class SenderReceiverSiteController : ControllerBase
    {
        private readonly ILogger<SenderReceiverSiteController> _logger;

        public SenderReceiverSiteController(ILogger<SenderReceiverSiteController> logger)
        {
            _logger = logger;
        }


        [HttpPost("SenderReceiver/Site")]
        public async Task<IActionResult> AddEditSenderReceiverSite([FromBody] object site)
        {
            try
            {
                if (!Request.Headers.TryGetValue("UserId", out var headerValues) ||
                    !int.TryParse(headerValues.FirstOrDefault(), out int userId))
                {
                    var message = "Invalid or missing UserId header parameter";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                }

                if (site == null)
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
                  JsonSerializer.Serialize(site),
                  new string('-', 200)
                  );



                var (msg, error) = await GTRR_HelperDAL.AddEditSenderReceiverSite(userId, JsonSerializer.Serialize(site));

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
                    SentrySdk.CaptureMessage($"AddEditSenderReceiverSite failed. Error: {error} | Msg: {msg}", SentryLevel.Error);

                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                    return BadRequest(new { message = responseMessage });

                }

                _logger.LogInformation(
                     "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                     "AddEditSenderReceiverSite",
                     "200 OK",
                     new string('-', 200)
                 );

               

                return Ok();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in AddEditSenderReceiverSite");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("SenderReceiver/Site")]
        [GzipCompression]
        public async Task<IActionResult> GetSenderReceiverSiteById()
        {
            try { 
            if (!Request.Headers.TryGetValue("UserId", out var userIdHeader) ||
                !int.TryParse(userIdHeader.FirstOrDefault(), out int loggedUser))
            {
                    var message = "Invalid or missing UserId header parameter";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                }


            if (!Request.Headers.TryGetValue("SenderReceiverSiteId", out var idHeader) ||
                !int.TryParse(idHeader.FirstOrDefault(), out int SenderReceiverSiteId))
            {
                    var message = "Invalid or missing SenderReceiverSiteId header.";
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
            var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceiverSiteByIdAsync(loggedUser, SenderReceiverSiteId);

            if (!string.IsNullOrEmpty(error))
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
                    SentrySdk.CaptureMessage($"GetSenderReceiverSiteById failed. Error: {error} | Msg: {msg}", SentryLevel.Error);  

                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage }


                );
            }


            var data = JsonSerializer.Deserialize<object>(json);
            _logger.LogInformation(
                 "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                 "GetSenderReceiverSiteById",
                 "200 OK",
                 new string('-', 200)
             );
            

                return Ok(data);
        }
        catch (Exception ex)
    {
 
        _logger.LogError(ex, "Unhandled exception in GetSenderReceiverSiteById");
        SentrySdk.CaptureException(ex);
 
        return StatusCode(500, new { message = "Internal server error" });
    }
        }
    }
}
