using RM2.Orm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Framework
{
    public class dbUtil
    {
        public static MyDb _myDb = null;
        public static void dbinit()
        {
            _myDb = new MyDb("DataSource=.;Database=RM2;USER ID=sa;Password=619239652");
        }
    }
}
