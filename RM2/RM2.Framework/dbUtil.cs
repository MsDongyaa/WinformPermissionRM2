using MyMiniOrm;
using MyMiniOrm.Commons;
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
            
            _myDb = new MyDb("Data Source = .;Initial Catalog = RM2;User Id = sa;Password = 619239652;");
        }
    }
}
