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
        private readonly GTRR_HelperDAL _helperDal;

        private readonly ILogger<AssetsController> _logger;

        public AssetsController(GTRR_HelperDAL helperDal,ILogger<AssetsController> logger)
        {
            _helperDal = helperDal;
            _logger = logger;
        }



        [HttpGet("VehicleTypes")]
        [GzipCompression]
        public async Task<IActionResult> VehicleTypes()
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




            var (json, msg, error) = await GTRR_HelperDAL.GetVehicleTypes(userId);

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
                    SentrySdk.CaptureMessage($"GetVehicleTypes failed. Error: {error} | Msg: {msg}", SentryLevel.Error);
                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage });
            }

           

            var data= JsonSerializer.Deserialize<object>(json);

            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "GetVehicleTypes",
                    "200 OK",
                    new string('-', 200)
                );
              
                return Ok(data);
        }

            catch (Exception ex)
        {
 
            _logger.LogError(ex, "Unhandled exception in GetVehicleTypes");
            SentrySdk.CaptureException(ex);
 
            return StatusCode(500, new { message = "Internal server error" });
        }
}

        [HttpPost("VehicleType")]
        public async Task<IActionResult> AddEditVehicleType([FromBody] object vehicletype)
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



            var (msg, error) = await GTRR_HelperDAL.AddEditVehicleType(userId, JsonSerializer.Serialize(vehicletype));

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
                    SentrySdk.CaptureMessage($"SenderReceivers failed. Error: {error} | Msg: {msg}", SentryLevel.Error);
                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage });
            }

            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "AddEditVehicleType",
                    "200 OK",
                    new string('-', 200)
                );
        

                return Ok();
        }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in AddEditVehicleType");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("States")]
        [GzipCompression]
        public async Task<IActionResult> GetStates()
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



            var (json, msg, error) = await GTRR_HelperDAL.GetStates(userId);

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
                    SentrySdk.CaptureMessage($"GetStates failed. Error: {error} | Msg: {msg}", SentryLevel.Error);
                    var responseMessage = string.IsNullOrEmpty(msg) ? "An unexpected error occurred." : msg;

                return BadRequest(new { message = responseMessage });
              
            }

            _logger.LogInformation("Returning compressed response for GetStates.");



            var data = JsonSerializer.Deserialize<object>(json);
            _logger.LogInformation(
                    "\nResponse: [{Action}] [{StatusCode}]\n{Separator}",
                    "GetStates",
                    "200 OK",
                    new string('-', 200)
                );
          
                return Ok(data);
        }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in GetStates");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }



        [HttpGet("PalletManagement")]
        [GzipCompression]
        public async Task<IActionResult> PalletManagement()
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
                userId
            );

            var (data, error) = await _helperDal.GetActivePalletManagementAsync(userId);
    

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogInformation(
                    "\nMethod: {Method}" +
                    "\nRequest: {Url}" +
                    "\nHeader:\nUserId={UserId}\nError: {Error}",
                    Request.Method,
                    $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}",
                    Request.Headers["UserId"].ToString(),
                    error
                   
                );
                    SentrySdk.CaptureMessage($"PalletManagement failed. Error: {error}", SentryLevel.Error);
                    var responseMessage ="An unexpected error occurred.";

                return BadRequest(new { message = responseMessage });
            }

            _logger.LogInformation(
                "\nResponse: [PalletManag] [200 OK]" +
                "\n{Separator}",
                new string('-', 200)
            );
               
                return Ok(data); 
        }

            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandled exception in PalletManagement");
                SentrySdk.CaptureException(ex);

                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
