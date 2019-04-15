using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MyMiniOrm.Commons
{
    public class DbKvs : List<KeyValuePair<string, object>>
    {
        public static DbKvs New()
        {
            return new DbKvs();
        }

        public DbKvs Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
            return this;
        }

        public List<SqlParameter> ToSqlParameters(string prefix = "@")
        {
            var result = new List<SqlParameter>();
            return this.Select(kv => new SqlParameter($"{prefix}{kv.Key}", kv.Value)).ToList();
        }
    }
}
