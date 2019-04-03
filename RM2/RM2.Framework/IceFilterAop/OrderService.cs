using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Framework.IceFilterAop
{
    public class OrderService : IBaseController,IOrderService
    {
        [LogFilter]
        public void Index(int id, string name)
        {
            Console.WriteLine("执行业务！");
        }

        public void XX()
        {
            Console.WriteLine("OO");
        }
    }
}
