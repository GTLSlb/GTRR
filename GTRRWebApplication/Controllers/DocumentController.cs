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
            if (!Request.Headers.TryGetValue("UserId", out var headerValue) ||
                !int.TryParse(headerValue.FirstOrDefault(), out int userId))
            {
                _logger.LogWarning("Invalid or missing UserId header parameter.");
                return BadRequest("Invalid or missing UserId header parameter");
            }

            _logger.LogInformation("Request: {Path}, UserId: {UserId}, Method: {Method}",
                Request.Path, userId, Request.Method);


            var (json, msg, error) = await GTRR_HelperDAL.GetRequiredDocs(userId);

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Bad Request: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Returning compressed response for GetVehicleTypes.");


            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }



        [HttpPost("RequiredDocs")]
        public async Task<IActionResult> AddEditRequiredDocuments([FromBody] object vehicletype)
        {
            if (!Request.Headers.TryGetValue("UserId", out var headerValues) ||
                !int.TryParse(headerValues.FirstOrDefault(), out int userId))
            {
                return BadRequest("Invalid or missing UserId header parameter");
            }

            if (vehicletype == null)
            {
                return BadRequest("Invalid or missing request body");
            }


            _logger.LogInformation("Request: {Method} {Path}", Request.Method, Request.Path);
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("Body: {Body}", JsonSerializer.Serialize(vehicletype));



            var (msg, error) = await GTRR_HelperDAL.AddEditRequiredDocuments(userId, JsonSerializer.Serialize(vehicletype));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Response: [400 Bad Request] Msg: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Response: [200 OK]");


            return Ok();
        }

    }
}
