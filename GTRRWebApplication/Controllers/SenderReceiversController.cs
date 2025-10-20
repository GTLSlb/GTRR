using GTRR_DataAccessLayer;
using GTRRWebApplication.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GTRRWebApplication.Controllers
{
    [Route("api/GTRR/V1")]
    [ApiController]
    //[TokenAuthorize]
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
                _logger.LogWarning("Invalid or missing UserId header parameter.");
                return BadRequest("Invalid or missing UserId header parameter");
            }

            _logger.LogInformation("Request: {Path}, UserId: {UserId}, Method: {Method}",
                Request.Path, userId, Request.Method);

         
            var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceivers(userId);

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Bad Request: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Returning compressed response for GetSenderReceivers.");


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

     
            _logger.LogInformation("Request: {Method} {Path}", Request.Method, Request.Path);
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("Body: {Body}", JsonSerializer.Serialize(senderreceiver));

        

            var (id,msg, error) = await GTRR_HelperDAL.AddEditSenderReceiver(userId, JsonSerializer.Serialize(senderreceiver));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Response: [400 Bad Request] Msg: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Response: [200 OK]");


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

            var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceiverByIdAsync(loggedUser, senderReceiverId);

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }


    }
}
