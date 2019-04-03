using RM2.Framework.IceAop;
using RM2.Framework.IceFilterAop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace RM2.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("控制台测试程序");
            //{
            //    IAopBehavior aopBehavior = AopExtend.Container().Resolve<IAopBehavior>();
            //    aopBehavior.Show();
            //}

            {
                AOPManager.Index("OrderService", "Index",new object[] { 999,"毕妍婷"});
            }

            Console.Read();
        }
    }
}
