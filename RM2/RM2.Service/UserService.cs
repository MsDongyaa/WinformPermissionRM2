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
using System.Linq.Expressions;

namespace RM2.Service
{
    public class UserService : IUserService
    {
        #region  用户表相关
        /// <summary>
        /// 获取用户列表数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="recordcount">总记录数</param>
        /// <returns></returns>
        public List<Base_User> GetUserList(PageModel page,out int recordcount)
        {
            recordcount = 0;
            var userlist = dbUtil._myDb.PageList<Base_User>(page.pageIndex, page.pageIndex, out recordcount, x=>x.DeleteMark!=1, x => x.CreateDate, MyDbSort.Desc);
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

        #endregion


        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<Base_UserRoleMap> AddUserRole(int userid)
        {
            var userrole = dbUtil._myDb.Query<Base_UserRoleMap>().Where(x => x.UserID == userid).ToList();
            return userrole;
        }



        /// <summary>
        /// 设置用户角色
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public int AddUserRole(int userid,int roleid)
        {
            var userrole = new Base_UserRoleMap();
            userrole.RoleID = roleid;
            userrole.UserID = userid;
            return dbUtil._myDb.InsertIfNotExists(userrole,x=>x.UserID==userid && x.RoleID==roleid);
        }


        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="roleid">角色id</param>
        /// <param name="userid">用户id</param>
        public void DeleteUserRole(int roleid, int userid)
        {
            var m = dbUtil._myDb.Query<Base_UserRoleMap>().Where(x => x.UserID == userid && x.RoleID == roleid).FirstOrDefault();
            if (m != null)
                dbUtil._myDb.Delete<Base_UserRoleMap>(m.ID);
        }


        /// <summary>
        /// 获取用户能看到的所有的菜单
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<Base_Menu> GetUserMenuList(int userid)
        {
            var userrole = dbUtil._myDb.Query<Base_UserRoleMap>().Where(x => x.UserID == userid).ToList().Select(x=>x.RoleID).ToList();
            if (userrole != null)
            {
                //这里无法转换成 List<Base_Menu> 类型 ，这个Select 无效
                //List<Base_Menu> usermenulist = dbUtil._myDb.Query<Base_RoleMenuMap>().Where(x => x.RoleID == userrole.RoleID).Include(x=>x.Menu).Select<Base_Menu>(x => x.Menu).ToList();
                //return usermenulist;
                var usermenulist = dbUtil._myDb.Query<Base_RoleMenuMap>().Where(x => userrole.Contains( x.RoleID ) ).Include(x => x.Menu).ToList().ConvertAll(x=>x.Menu);
                return usermenulist;
            }
            else
            {
                List<Base_Menu> usermenulist = dbUtil._myDb.Query<Base_Menu>().Where(x => x.DeleteMark != 1).ToList();
                return usermenulist;
            }
        }

    }
}
