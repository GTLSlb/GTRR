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
            if (!Request.Headers.TryGetValue("UserId", out var headerValue) ||
                !int.TryParse(headerValue.FirstOrDefault(), out int userId))
            {
                
                return BadRequest("Invalid or missing UserId header parameter");
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
       


        [HttpPost("SenderReceiver")]
        public async Task<IActionResult> AddEditSenderReceivers([FromBody] object senderreceiver)
        {
            if (!Request.Headers.TryGetValue("UserId", out var headerValues) ||
                !int.TryParse(headerValues.FirstOrDefault(), out int userId))
            {
                return BadRequest("Invalid or missing UserId header parameter");
            }

            if (senderreceiver == null)
            {
                return BadRequest("Invalid or missing request body");
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




            var (id,msg, error) = await GTRR_HelperDAL.AddEditSenderReceiver(userId, JsonSerializer.Serialize(senderreceiver));

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
                var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage });
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

        [HttpGet("SenderReceiver")]
        [GzipCompression]
        public async Task<IActionResult> GetSenderReceiverById()
        {
            
            if (!Request.Headers.TryGetValue("UserId", out var userIdHeader) ||
                !int.TryParse(userIdHeader.FirstOrDefault(), out int loggedUser))
            {
                return BadRequest("Invalid or missing UserId header.");
            }

           
            if (!Request.Headers.TryGetValue("SenderReceiverId", out var idHeader) ||
                !int.TryParse(idHeader.FirstOrDefault(), out int senderReceiverId))
            {
                return BadRequest("Invalid or missing SenderReceiverId header.");
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


    }
}
