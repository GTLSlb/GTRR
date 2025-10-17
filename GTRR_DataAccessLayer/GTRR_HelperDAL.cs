using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRR_DataAccessLayer
{
    public static class GTRR_HelperDAL
    {
        private static readonly string gtrrConnectionString;
        private static readonly int commandTimeout = 120;


        static GTRR_HelperDAL()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            gtrrConnectionString = config.GetConnectionString("GTRRConnectionString");
        }

        public static bool ValidateUserToken(int userId, string token, out bool validation)
        {
            validation = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(gtrrConnectionString))
                using (SqlCommand command = new SqlCommand("GTRR_TokenValidation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;

                    command.Parameters.Add(new SqlParameter("@USER_ID", userId));
                    command.Parameters.Add(new SqlParameter("@TOKEN", token));

                    SqlParameter isValidParam = new SqlParameter("@IS_VALID", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(isValidParam);

                    connection.Open();
                    command.ExecuteNonQuery();

                    validation = (bool)(isValidParam.Value ?? false);
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

    }
}