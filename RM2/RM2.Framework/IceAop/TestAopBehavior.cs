using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Framework.IceAop
{
   public class TestAopBehavior: IAopBehavior
    {
        public void Show()
        {
            string name = "AA123";
            int i = Convert.ToInt32(name);
            Console.WriteLine("123456");
        }
    }
}
