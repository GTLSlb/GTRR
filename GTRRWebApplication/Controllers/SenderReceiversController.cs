using Azure.Core;
using GTRR_DataAccessLayer;
using GTRRWebApplication.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GTRRWebApplication.Controllers
{
    [Route("api/GTRR/V1")]
    [ApiController]
    [TokenAuthorize]
    public class SenderReceiversController : ControllerBase

    {
        private readonly ILogger<SenderReceiversController> _logger;
      
        public SenderReceiversController(ILogger<SenderReceiversController> logger)
        {
            _logger = logger;
        }


        [HttpGet("SenderReceivers")]
        [GzipCompression]
        public async Task<IActionResult> SenderReceivers()
        {
            try
            {
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



                var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceivers(userId);

                if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
                {
                    _logger.LogError(
                        "\nMethod: {Method}" +
                        "\nRequest: {Url}" +
                        "\nHeader:\nUserId={UserId}\nError: {Error}\nMessage: {Msg}",
                        Request.Method,
                        $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
                        Request.Headers["UserId"].ToString(),
                        error,
                        msg
                    );

                    
                    SentrySdk.CaptureMessage($"SenderReceivers failed. Error: {error} | Msg: {msg}", SentryLevel.Error);

                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;
                    return BadRequest(new { message = responseMessage });
                }

                _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "GetSenderReceivers",
                    "200 OK",
                    new string('-', 200)
                );

               

                var data = JsonSerializer.Deserialize<object>(json);
                return Ok(data);
            }
            catch (Exception ex)
            {
        
                _logger.LogError(ex, "Unhandled exception in SenderReceivers");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }



        [HttpPost("SenderReceiver")]
        public async Task<IActionResult> AddEditSenderReceivers([FromBody] object senderreceiver)
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

                if (senderreceiver == null)
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
                    JsonSerializer.Serialize(senderreceiver),
                    new string('-', 200)
                    );




                var (id, msg, error) = await GTRR_HelperDAL.AddEditSenderReceiver(userId, JsonSerializer.Serialize(senderreceiver));

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
                    SentrySdk.CaptureMessage($"AddEditSenderReceiver failed. Error: {error} | Msg: {msg}", SentryLevel.Error);
                    
                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                    return BadRequest(new { Message = responseMessage });
                }

                _logger.LogInformation(
                        "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                        "AddEditSenderReceiver",
                        "200 OK",
                        new string('-', 200)
                    );

              

                return Ok(new
                {
                    SenderReceiverId = id

                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in AddEditSenderReceiver");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("SenderReceiver")]
        [GzipCompression]
        public async Task<IActionResult> GetSenderReceiverById()
        {
            try
            {
                if (!Request.Headers.TryGetValue("UserId", out var userIdHeader) ||
                    !int.TryParse(userIdHeader.FirstOrDefault(), out int loggedUser))
                {
                    var message = "Invalid or missing UserId header parameter";
                    _logger.LogWarning(message);
                    SentrySdk.CaptureMessage(message, SentryLevel.Warning);
                    return BadRequest(message);
                }


                if (!Request.Headers.TryGetValue("SenderReceiverId", out var idHeader) ||
                    !int.TryParse(idHeader.FirstOrDefault(), out int senderReceiverId))
                {
                    var message = "Invalid or missing SenderReceiverId header";
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
                var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceiverByIdAsync(loggedUser, senderReceiverId);

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
                    SentrySdk.CaptureMessage($"SenderReceiverById failed. Error: {error} | Msg: {msg}", SentryLevel.Error);

                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                    return BadRequest(new { message = responseMessage });


                }
                var data = JsonSerializer.Deserialize<object>(json);
                _logger.LogInformation(
                     "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                     "GetSenderReceiverById",
                     "200 OK",
                     new string('-', 200)
                 );
              
                return Ok(data);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in SenderReceiverById");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            } 
        }


    }
}
