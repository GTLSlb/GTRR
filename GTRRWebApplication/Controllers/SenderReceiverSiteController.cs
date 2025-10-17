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
            if (!Request.Headers.TryGetValue("UserId", out var headerValues) ||
                !int.TryParse(headerValues.FirstOrDefault(), out int userId))
            {
                return BadRequest("Invalid or missing UserId header parameter");
            }

            if (site == null)
            {
                return BadRequest("Invalid or missing request body");
            }


            _logger.LogInformation("Request: {Method} {Path}", Request.Method, Request.Path);
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("Body: {Body}", JsonSerializer.Serialize(site));



            var (msg, error) = await GTRR_HelperDAL.AddEditSenderReceiverSite(userId, JsonSerializer.Serialize(site));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Response: [400 Bad Request] Msg: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Response: [200 OK]");
            return Ok();
        }

        [HttpGet("SenderReceiver/Site")]
        [GzipCompression]
        public async Task<IActionResult> GetSenderReceiverSiteById()
        {

            if (!Request.Headers.TryGetValue("UserId", out var userIdHeader) ||
                !int.TryParse(userIdHeader.FirstOrDefault(), out int loggedUser))
            {
                return BadRequest("Invalid or missing UserId header.");
            }


            if (!Request.Headers.TryGetValue("SenderReceiverSiteId", out var idHeader) ||
                !int.TryParse(idHeader.FirstOrDefault(), out int SenderReceiverSiteId))
            {
                return BadRequest("Invalid or missing SenderReceiverSiteId header.");
            }

            var (json, msg, error) = await GTRR_HelperDAL.GetSenderReceiverSiteByIdAsync(loggedUser, SenderReceiverSiteId);

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
    }
}
