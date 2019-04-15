
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using RM2.Framework.AopServe2;

namespace RM2.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("控制台测试程序");
            //{
            //IAopBehavior aopBehavior = AopExtend.Container().Resolve<>();
            //aopBehavior.Show();
            //}

            {
                AOPManager.Index("OrderService", "Index",new object[] { 999,"毕妍婷"});
            }

            Console.Read();
        }
    }
}
