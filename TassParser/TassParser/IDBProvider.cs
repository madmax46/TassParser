using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public interface IDBProvider
    {

        DataTable ProcedureByName(string procedure, params object[] par);

        DataSet ProcedureDSByName(string procedure, params object[] par);

        DataTable GetDataTable(string sqlQuery);
        int Execute(string query);

        string ToMySqlParam(object param);
    }
}
