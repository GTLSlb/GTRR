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




            var (json, msg, error) = await GTRR_HelperDAL.GetVehicleTypes(userId);

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogInformation(
                     "\nResponse: [{Action}] [{StatusCode}] {Message}" +
                     "\nOriginal Error: {Error}" +
                     "\n{Separator}",
                     "GetVehicleTypes",
                     "400 Bad Request",
                     msg,
                     error,
                     new string('-', 200)
                 );

                return BadRequest(msg);
            }

           


            var data = JsonSerializer.Deserialize<object>(json);
            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "GetVehicleTypes",
                    "200 OK",
                    new string('-', 200)
                );

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



            var (msg, error) = await GTRR_HelperDAL.AddEditVehicleType(userId, JsonSerializer.Serialize(vehicletype));

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(error))
            {
                _logger.LogInformation(
                     "\nResponse: [{Action}] [{StatusCode}] {Message}" +
                     "\nOriginal Error: {Error}" +
                     "\n{Separator}",
                     "AddEditVehicleType",
                     "400 Bad Request",
                     msg,
                     error,
                     new string('-', 200)
                 );


                return BadRequest(msg);
            }

            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "AddEditVehicleType",
                    "200 OK",
                    new string('-', 200)
                );


            return Ok();
        }


        [HttpGet("States")]
        [GzipCompression]
        public async Task<IActionResult> GetStates()
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
