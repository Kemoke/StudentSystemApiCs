using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;

namespace StudentSystemApiCs.Util
{
    /// <summary>
    /// Holds all configuration information about the app.
    /// </summary>
    public static class AppConfig
    {

        public static string AppKey { get; private set; }
        public static string HostUri { get; private set; }
        public static string LogFileName { get; private set; }
        public static Func<DbConnection> GetDbConnection { get; private set; }

        /// <summary>
        /// Loads configuration data from json file.
        /// </summary>
        /// <param name="workingDirectory">Path to config.json directory</param>
        public static void Init(string workingDirectory = "")
        {
            var configJson = File.ReadAllText(workingDirectory+"config.json");
            dynamic config = JsonConvert.DeserializeObject(configJson);
            string connType = config.DbType;
            string dbname = config.DbName;
            string srvname = config.ServerName;
            string userid = config.UserId;
            string password = config.Password;
            //This switch statement binds a return function for each of the different possible database connections
            switch (connType.ToUpper())
            {
                case "MYSQL":
                    var connectionString = new MySqlConnectionStringBuilder
                    {
                        Database = dbname,
                        UserID = userid,
                        Password = password,
                        Server = srvname
                    }.ConnectionString;
                    GetDbConnection = () => new MySqlConnection(connectionString); ;
                    break;
                case "MSSQL":
                    connectionString = new SqlConnectionStringBuilder
                    {
                        DataSource = srvname,
                        InitialCatalog = dbname,
                        UserID = userid,
                        Password = password
                    }.ConnectionString;
                    GetDbConnection = () => new SqlConnection(connectionString);
                    break;
                case "LOCALDB":
                    connectionString = dbname;
                    GetDbConnection = () => new LocalDbConnectionFactory("mssqllocaldb").CreateConnection(connectionString);
                    break;
            }
            AppKey = config.AppKey;
            HostUri = config.HostUri;
            LogFileName = config.LogFileName;
            using (var context = new UniContext())
            {
                context.Database.CreateIfNotExists();
            }
            UserCache.Reload();
        }


    }
}