using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMiniOrm
{
    public class MyMiniOrmConfiguration
    {
        private static string _defaultConnectionString;

        private static string _prefix;

        private static bool _hasInit;

        public static void Init(string connectionString, string prefix = "@")
        {
            if (_hasInit)
            {
                throw new Exception("MyMiniOrm只能初始化一次");
            }

            _defaultConnectionString = connectionString;
            _prefix = prefix;
            _hasInit = true;
        }

        public static string GetConnectionString()
        {
            return _defaultConnectionString;
        }

        public static string GetPrefix()
        {
            return _prefix;
        }
    }
}
