using RM2.Framework;
using RM2.IService;
using RM2.Model;
using RM2.Model.BusinesModel;
using RM2.Orm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Service
{
    public class UserService : IUserService
    {
        /// <summary>
        /// 获取用户列表数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="recordcount">总记录数</param>
        /// <returns></returns>
        public List<Base_User> GetUserList(PageModel page,out int recordcount)
        {
            recordcount = 0;
            var userlist = dbUtil._myDb.PageList<Base_User>(page.pageIndex, page.pageIndex, out recordcount, x=>x.DeleteMark!=1, x => x.CreateDate);
            return userlist;
        }


        public int AddUser(Base_User user)
        {
            return dbUtil._myDb.Insert(user);
        }



        public int UpdateUser(Base_User user)
        {
            return dbUtil._myDb.Update(user);
        }



        public int DeleteUser(Base_User user)
        {
            user.DeleteMark = 1;
            return dbUtil._myDb.Update(user);
        }
    }
}
