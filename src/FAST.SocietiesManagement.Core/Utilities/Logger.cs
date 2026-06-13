using System;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace FAST.SocietiesManagement.Core.Utilities
{
    public static class Logger
    {
        // Fallback file path if the database goes completely down
        private static readonly string _fallbackFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FatalFallback.txt");

        public static void LogError(Exception ex, string context = "", int? userId = null)
        {
            LogToDatabase("ERROR", context, $"{ex.Message} | StackTrace: {ex.StackTrace}", userId);
        }

        public static void LogInfo(string message, string context = "", int? userId = null)
        {
            LogToDatabase("INFO", context, message, userId);
        }

        private static void LogToDatabase(string level, string context, string message, int? userId)
        {
            try
            {
                // Retrieve native IP Configuration of local machine running the app
                string hostName = Dns.GetHostName();
                string ipAddress = Dns.GetHostEntry(hostName)
                                      .AddressList
                                      .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "Unknown IP";

                // We are in Core, but since DbConnectionProvider is in DAL, to avoid circular dependencies in strict N-Tier,
                // we assume the connection string is passed from DAL/BLL or we fetch it dynamically.
                // For direct DB access in core utility:
                string connStr = @"Server=localhost\SQLEXPRESS;Database=FASTSocietiesDB;Trusted_Connection=True;TrustServerCertificate=True;";

                using (var connection = new SqlConnection(connStr))
                {
                    var cmd = new SqlCommand(@"INSERT INTO SystemAuditLogs (LogLevel, SourceContext, Message, IPAddress, MachineName, UserID) 
                                             VALUES (@Level, @Context, @Message, @IP, @Machine, @UserID)", connection);
                    cmd.Parameters.AddWithValue("@Level", level);
                    cmd.Parameters.AddWithValue("@Context", string.IsNullOrEmpty(context) ? DBNull.Value : (object)context);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@IP", ipAddress);
                    cmd.Parameters.AddWithValue("@Machine", hostName);
                    cmd.Parameters.AddWithValue("@UserID", userId.HasValue ? (object)userId.Value : DBNull.Value);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception fallbackEx)
            {
                // Pure Enterprise Fallback: If DB is down, write to flat file
                string fallbackLog = $"[{DateTime.Now}] DB FAILURE - Attempted: {level} | OrigMsg: {message} | SysError: {fallbackEx.Message}{Environment.NewLine}";
                File.AppendAllText(_fallbackFilePath, fallbackLog);
            }
        }
    }
}
