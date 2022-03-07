using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace TassParserLib
{

    public class MySQLConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }
        public string Database { get; set; }
        public string SslMode { get; set; }
        public string CharacterSet { get; set; }

        public string CreateConnectionString()
        {
            List<string> stringConnect = new List<string>();
            stringConnect.Add($"Host={Host}");
            if (Port != 0)
                stringConnect.Add($"Port={Port}");
            stringConnect.Add($"User Id={UserId}");
            stringConnect.Add($"Password={Password}");

            if (!string.IsNullOrEmpty(Database))
                stringConnect.Add($"Database={Database}");

            if (!string.IsNullOrEmpty(SslMode))
                stringConnect.Add($"SslMode={SslMode}");

            if (!string.IsNullOrEmpty(CharacterSet))
                stringConnect.Add($"Character Set={CharacterSet}");

            return string.Join(";", stringConnect);
        }
    }

    public class MySQLWrap : IDBProvider
    {
        private string _connectionStr;

        //static string connectionStr = "User Id=root;Password=admin;Host=localhost;Database=stockquotes;SslMode=none;Character Set=cp1251";
        public string ConnectionStr { get => _connectionStr; private set => _connectionStr = value; }
        public MySQLConfig ConnectConfig { get; private set; }
        public MySQLWrap(MySQLConfig config)
        {
            ConnectConfig = config;
            ConnectionStr = ConnectConfig.CreateConnectionString();
        }
        public DataTable ProcedureByName(string procedure, params object[] par)
        {
            string parPart = string.Join(",", par.Select(r => string.Format("{0}", Convert.ToString(r))).ToArray());
            string sql = string.Format("CALL {0}({1})", procedure, parPart);
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, connect))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        public DataSet ProcedureDSByName(string procedure, params object[] par)
        {
            string parPart = string.Join(",", par.Select(r => string.Format("{0}", Convert.ToString(r))).ToArray());
            string sql = string.Format("CALL {0}({1})", procedure, parPart);
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, connect))
                {
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset);
                    return dataset;
                }
            }
        }


        public DataTable GetDataTable(string sqlQuery)
        {
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sqlQuery, connect))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }
        public int Execute(string query)
        {
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlCommand com = new MySqlCommand(query, connect))
                {
                    return com.ExecuteNonQuery();
                }
            }
        }

        public string ToMySqlParam(object param)
        {
            return ToMySqlParamStat(param.ToString());
        }

        public static string ToMySqlParamStat(object param)
        {
            if (param is double || param is decimal || param is float)
                return ToMySqlParamStat(param.ToString().Replace(',', '.'));

            if (param is int || param is long)
                return param.ToString();

            if (param is Enum)
                return ToMySqlParamStat(Convert.ToInt32(param));

            if (param is DateTime)
                return ToMySqlParamStat(((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss"));

            if (param as string != null)
            {
                string retVal = Convert.ToString(param);
                retVal = retVal.Replace("'", "\\'");
                return string.Format("'{0}'", retVal);
            }

            if (param == null)
                return "NULL";

            if (param is bool)
                return Convert.ToBoolean(param) == true ? ToMySqlParamStat(1) : ToMySqlParamStat(0);

            return ToMySqlParamStat(param.ToString());
        }

    }
}
