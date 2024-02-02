using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace DotnetAPI.Models
{
   
    public partial class MCommand
    {
        public static List<Dictionary<string, object>> ToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return list;
        }
        public static Dictionary<string, object> ToJSONSingleRow(DataTable table)
        {
            var dict = new Dictionary<string, object>();

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];

                }
                break;
            }
            return dict;
        }

    }



}