using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using FrameWork;
using FrameWork.FeaturesServe;

namespace RM2.Framework.AopServe2
{
    public static class AOPManager
    {
        static AOPManager()
        {
            //获取当前程序的基目录
           string url= AppDomain.CurrentDomain.BaseDirectory;
            //获取目录中所有的目录名称
            string [] fileNameList= Directory.GetFiles(url);
            //遍历exe或dll结尾的文件名称
            foreach (var item in fileNameList.Where(f=>f.EndsWith("exe")||f.EndsWith("dll")))
            {
                //反射
                Assembly assembly = Assembly.Load(Path.GetFileNameWithoutExtension(item));
                //得到类名集合
                foreach (var type in assembly.GetTypes())
                {
                    //指定类有无继承该接口
                    if (typeof(IBaseController).IsAssignableFrom(type))
                    {
                        //继承该接口的子类名称放进容器
                        _ServiceList.Add(type.Name,type);
                    }
                }
            }
        }

        //容器
        private static Dictionary<string, Type> _ServiceList = new Dictionary<string, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClassName">实现类名称</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="paramters">参数</param>
        public static void Index(string ClassName,string methodName,params object[] paramters)
        {
            //获取实现类
            Type type = _ServiceList[ClassName];
            //创建实例
            var oService = Activator.CreateInstance(type);
            //获取方法名
            var method = type.GetMethod(methodName);
            //检测该方法是否调用日志特性
            if (method.IsDefined(typeof(LogHelperAttribute),true)) 
            {
              var attribute=  (LogHelperAttribute)method.GetCustomAttribute(typeof(LogHelperAttribute), true);
                //写个日志
                attribute.Write("写个日志");
            }
            //其他操作
            method.Invoke(oService,paramters);
        }
    }
}
