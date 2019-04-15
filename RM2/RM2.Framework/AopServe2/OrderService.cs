using FrameWork;
using FrameWork.FeaturesServe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Framework.AopServe2
{
    public class OrderService : IBaseController,IOrderService
    {
        [LogHelper]
        public void Index(int id, string name)
        {
            Console.WriteLine("执行业务！");
        }

        [LogHelper]
        public void XX()
        {
            Console.WriteLine("执行业务二");
        }
    }
}
