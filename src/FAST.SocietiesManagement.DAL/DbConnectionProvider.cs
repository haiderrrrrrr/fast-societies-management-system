using System;
using Microsoft.Data.SqlClient;

namespace FAST.SocietiesManagement.DAL
{
    public static class DbConnectionProvider
    {
        private const string DefaultConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=FASTSocietiesDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("FAST_SOCIETIES_CONNECTION_STRING")
            ?? DefaultConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
