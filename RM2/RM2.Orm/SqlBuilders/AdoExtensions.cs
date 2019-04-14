using System.Collections.Generic;
using System.Data.SqlClient;

namespace MyMiniOrm.SqlBuilders
{
    public static class AdoExtensions
    {
        public static List<SqlParameter> ToSqlParameters(this List<KeyValuePair<string, object>> kvs)
        {
            var result = new List<SqlParameter>();

            if (kvs.Count > 0)
            {
                foreach (var kv in kvs)
                {
                    result.Add(new SqlParameter(kv.Key, kv.Value));
                }
            }

            return result;
        }
    }
}
