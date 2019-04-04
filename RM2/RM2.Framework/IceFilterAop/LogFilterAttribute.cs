using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Framework.IceFilterAop
{
   public class LogFilterAttribute:Attribute
    {
        public void Show()
        {
            Console.WriteLine("写个日志玩玩！");
        }
    }
}
