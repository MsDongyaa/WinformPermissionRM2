using RM2.Framework;
using RM2.IService;
using RM2.Model;
using RM2.Model.BusinesModel;
using MyMiniOrm.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Service
{
    public class LogService : ILogService
    {
        /// <summary>
        /// 获取日志列表数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="recordcount">总记录数</param>
        /// <returns></returns>
        public List<Base_Log> GetLogList(PageModel page,out int recordcount)
        {
            recordcount = 0;
            var userlist = dbUtil._myDb.PageList<Base_Log>(page.pageIndex, page.pageIndex, out recordcount, x=>x.DeleteMark!=1, x=>x.ID,MyDbSort.Desc);
            return userlist;
        }


        public int AddLog(Base_Log user)
        {
            return dbUtil._myDb.Insert(user);
        }



        public int UpdateLog(Base_Log user)
        {
            return dbUtil._myDb.Update(user);
        }



        public int DeleteLog(Base_Log user)
        {
            user.DeleteMark = 1;
            return dbUtil._myDb.Update(user);
        }
    }
}
