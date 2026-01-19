using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace GTRR_DataAccessLayer
{
    public class GTRR_HelperDAL
    {
        private static readonly string? gtrrConnectionString;
        private static readonly int commandTimeout = 120;

        private readonly GTRR_DbContext _context;

        public GTRR_HelperDAL(GTRR_DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        static GTRR_HelperDAL()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            gtrrConnectionString = config.GetConnectionString("GTRRConnectionString");
        }

        public static bool ValidateUserToken(int userId, string token, out bool validation, out string? message)
        {
            validation = false;
            message = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(gtrrConnectionString))
                using (SqlCommand command = new SqlCommand("GTRR_TokenValidation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;

                    command.Parameters.AddWithValue("@USER_ID", userId);
                    command.Parameters.AddWithValue("@TOKEN", token);

                    var isValidParam = new SqlParameter("@IS_VALID", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(isValidParam);

                    var msgParam = new SqlParameter("@MSG", SqlDbType.NVarChar, 200)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(msgParam);

                    connection.Open();
                    command.ExecuteNonQuery();

                    validation = (bool)(isValidParam.Value ?? false);
                    message = msgParam.Value?.ToString();

                    return validation;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error in ValidateUserToken: " + ex.Message);
                return false;
            }
        }



        public static async Task<(string Json, string Msg, string ErrorMessage)> GetSenderReceivers(int? userId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetSenderReceivers", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };

                command.Parameters.Add(new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value));

                var reportParam = new SqlParameter("@SENDER_RECEIVERS", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(reportParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = reportParam.Value?.ToString() ?? "";
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "[]";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", "", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }

        }

        public static async Task<(int SenderReciver_Id,string Msg, string ErrorMessage)> AddEditSenderReceiver(int? userId, string senderreceiver)
        {
            string msg = "";
            string errorMessage = "";
            int senderReceiverId = 0;
            try
            {
                var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value),
                new SqlParameter("@SENDER_RECEIVER", senderreceiver ?? (object)DBNull.Value)
            };

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                parameters.Add(msgParam);

                var idParam = new SqlParameter("@SENDER_RECEIVER_ID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                parameters.Add(idParam);

                using (var connection = new SqlConnection(gtrrConnectionString))
                using (var command = new SqlCommand("AddEditSenderReceiver", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddRange(parameters.ToArray());

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    msg = msgParam.Value?.ToString() ?? "";
                    senderReceiverId = idParam.Value != DBNull.Value ? Convert.ToInt32(idParam.Value) : 0;

                }

                return (senderReceiverId, msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return (0,"", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "An error occurred while executing the SQL command: " + ex.Message;
                return (0,"", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred: " + ex.Message;
                return (0,"", errorMessage);
            }
        }

        public static async Task<(string Json, string Msg, string ErrorMessage)> GetSenderReceiverByIdAsync(int? userId, int? senderReceiverId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetSenderReceiverById", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };

              
                command.Parameters.Add(new SqlParameter("@LOGGED_USER", userId));
                command.Parameters.Add(new SqlParameter("@SENDER_RECEIVER_ID", senderReceiverId));

              
                var jsonParam = new SqlParameter("@JSON", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(jsonParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

                // Execute
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = jsonParam.Value?.ToString() ?? "{}"; 
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "{}";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }
        }

        public static async Task<(string Msg, string ErrorMessage)> AddEditSenderReceiverSite(int? userId, string Site)
        {
            string msg = "";
            string errorMessage = "";

            try
            {
                var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value),
                new SqlParameter("@SITE", Site ?? (object)DBNull.Value)
            };

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                parameters.Add(msgParam);

                using (var connection = new SqlConnection(gtrrConnectionString))
                using (var command = new SqlCommand("AddEditSenderReceiverSite", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddRange(parameters.ToArray());

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    msg = msgParam.Value?.ToString() ?? "";
                }

                return (msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "An error occurred while executing the SQL command: " + ex.Message;
                return ("", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred: " + ex.Message;
                return ("", errorMessage);
            }
        }


        public static async Task<(string Json, string Msg, string ErrorMessage)> GetSenderReceiverSiteByIdAsync(int? userId, int? senderReceiverSiteId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetSenderReceiverSiteById", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };


                command.Parameters.Add(new SqlParameter("@LOGGED_USER", userId));
                command.Parameters.Add(new SqlParameter("@SITE_ID", senderReceiverSiteId));


                var jsonParam = new SqlParameter("@JSON", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(jsonParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

               
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = jsonParam.Value?.ToString() ?? "{}";
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "{}";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }
        }






        public static async Task<(string Json, string Msg, string ErrorMessage)> GetVehicleTypes(int? userId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetVehicleTypes", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };

                command.Parameters.Add(new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value));

                var reportParam = new SqlParameter("@JSON", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(reportParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = reportParam.Value?.ToString() ?? "";
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "[]";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", "", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }

        }

        public static async Task<(string Msg, string ErrorMessage)> AddEditVehicleType(int? userId, string vehicletype)
        {
            string msg = "";
            string errorMessage = "";
           
            try
            {
                var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value),
                new SqlParameter("@VEHICLE_TYPE", vehicletype ?? (object)DBNull.Value)
            };

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                parameters.Add(msgParam);

                

                using (var connection = new SqlConnection(gtrrConnectionString))
                using (var command = new SqlCommand("AddEditVehicleType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddRange(parameters.ToArray());

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    msg = msgParam.Value?.ToString() ?? "";
                    

                }

                return( msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ( "", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "An error occurred while executing the SQL command: " + ex.Message;
                return ("", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred: " + ex.Message;
                return ( "", errorMessage);
            }
        }

        public static async Task<(string Json, string Msg, string ErrorMessage)> GetStates(int? userId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetStates", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };

                command.Parameters.Add(new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value));

                var reportParam = new SqlParameter("@JSON", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(reportParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = reportParam.Value?.ToString() ?? "";
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "[]";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", "", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }

        }

        public static async Task<(string Json, string Msg, string ErrorMessage)> GetRequiredDocs(int? userId)
        {
            string json = "";
            string msg = "";
            string errorMessage = "";

            try
            {
                using var connection = new SqlConnection(gtrrConnectionString);
                using var command = new SqlCommand("GetRequiredDocs", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = commandTimeout
                };

                command.Parameters.Add(new SqlParameter("@LOGGED", userId ?? (object)DBNull.Value));

                var reportParam = new SqlParameter("@JSON", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(reportParam);

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(msgParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                json = reportParam.Value?.ToString() ?? "";
                msg = msgParam.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "[]";
                }

                return (json, msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", "", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "SQL Error: " + ex.Message;
                return ("", "", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "Unexpected Error: " + ex.Message;
                return ("", "", errorMessage);
            }

        }

        public static async Task<(string Msg, string ErrorMessage)> AddEditRequiredDocuments(int? userId, string requiredDoc)
        {
            string msg = "";
            string errorMessage = "";

            try
            {
                var parameters = new List<SqlParameter>
            {
                new SqlParameter("@LOGGED_USER", userId ?? (object)DBNull.Value),
                new SqlParameter("@REQ_DOCS", requiredDoc ?? (object)DBNull.Value)
            };

                var msgParam = new SqlParameter("@MSG", SqlDbType.VarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                parameters.Add(msgParam);



                using (var connection = new SqlConnection(gtrrConnectionString))
                using (var command = new SqlCommand("AddEditRequiredDocuments", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;
                    command.Parameters.AddRange(parameters.ToArray());

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    msg = msgParam.Value?.ToString() ?? "";


                }

                return (msg, errorMessage);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                errorMessage = "Execution Timeout Expired. Please try again later or optimize your query.";
                return ("", errorMessage);
            }
            catch (SqlException ex)
            {
                errorMessage = "An error occurred while executing the SQL command: " + ex.Message;
                return ("", errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred: " + ex.Message;
                return ("", errorMessage);
            }
        }


        public async Task<(List<PalletManagement>? Data, string? Error)> GetActivePalletManagementAsync(int? userId)
        {
            if (userId == null)
                return (null, "Invalid User ID");

            try
            {
                var userExists = await _context.Database
            .ExecuteSqlInterpolatedAsync(
                $"SELECT 1 WHERE EXISTS (SELECT 1 FROM dbo.GTAM_Users WHERE USER_ID = {userId} AND STATUS_ID = 1)");

                if (userExists == 0)
                    return (null, "Invalid User ID");

            

                var data = await _context.PalletManagements
                    .Where(p => p.STATUS_ID == 1)
                    .OrderBy(p => p.PALLET_MGMT_NAME)
                    .ToListAsync();


                return (data, null);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                return (null, "Execution Timeout Expired. Please try again later or optimize your query");
            }
            catch (SqlException ex)
            {
                return (null, "SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (null, "An error occurred: " + ex.Message);
            }
        }
    }


}