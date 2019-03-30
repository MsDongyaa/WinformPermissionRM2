using System.Collections.Generic;

namespace RM2.Orm.Commons
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
    }
}
