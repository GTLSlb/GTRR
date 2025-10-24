using GTRR_DataAccessLayer;
using GTRRWebApplication.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GTRRWebApplication.Controllers
{
    [Route("api/GTRR/V1")]
    [ApiController]
    //[TokenAuthorize]
    public class AssetsController : ControllerBase
    {
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(ILogger<AssetsController> logger)
        {
            _logger = logger;
        }



        [HttpGet("VehicleTypes")]
        [GzipCompression]
        public async Task<IActionResult> VehicleTypes()
        {
            if (!Request.Headers.TryGetValue("UserId", out var headerValue) ||
                !int.TryParse(headerValue.FirstOrDefault(), out int userId))
            {
                _logger.LogWarning("Invalid or missing UserId header parameter.");
                return BadRequest("Invalid or missing UserId header parameter");
            }

            _logger.LogInformation("Request: {Path}, UserId: {UserId}, Method: {Method}",
                Request.Path, userId, Request.Method);


            var (json, msg, error) = await GTRR_HelperDAL.GetVehicleTypes(userId);

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Bad Request: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Returning compressed response for GetVehicleTypes.");


            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }



        [HttpPost("VehicleType")]
        public async Task<IActionResult> AddEditVehicleType([FromBody] object vehicletype)
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



            var (msg, error) = await GTRR_HelperDAL.AddEditVehicleType(userId, JsonSerializer.Serialize(vehicletype));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Response: [400 Bad Request] Msg: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Response: [200 OK]");


            return Ok();
        }


        [HttpGet("States")]
        [GzipCompression]
        public async Task<IActionResult> GetStates()
        {
            if (!Request.Headers.TryGetValue("UserId", out var headerValue) ||
                !int.TryParse(headerValue.FirstOrDefault(), out int userId))
            {
                _logger.LogWarning("Invalid or missing UserId header parameter.");
                return BadRequest("Invalid or missing UserId header parameter");
            }

            _logger.LogInformation("Request: {Path}, UserId: {UserId}, Method: {Method}",
                Request.Path, userId, Request.Method);


            var (json, msg, error) = await GTRR_HelperDAL.GetStates(userId);

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Bad Request: {Msg}, Error: {Error}", msg, error);
                return BadRequest(msg);
            }

            _logger.LogInformation("Returning compressed response for GetStates.");


            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }


    }
}
