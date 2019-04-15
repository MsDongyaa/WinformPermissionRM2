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
    public class RoleService : IRoleService
    {
        /// <summary>
        /// 获取角色列表数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="recordcount">总记录数</param>
        /// <returns></returns>
        public List<Base_Role> GetRoleList(PageModel page,out int recordcount)
        {
            recordcount = 0;
            var userlist = dbUtil._myDb.PageList<Base_Role>(page.pageIndex, page.pageIndex, out recordcount, x=>x.DeleteMark!=1, x => x.CreateDate, MyDbSort.Desc);
            return userlist;
        }


        public int AddRole(Base_Role user)
        {
            return dbUtil._myDb.Insert(user);
        }



        public int UpdateRole(Base_Role user)
        {
            return dbUtil._myDb.Update(user);
        }



        public int DeleteRole(Base_Role user)
        {
            user.DeleteMark = 1;
            return dbUtil._myDb.Update(user);
        }






        /// <summary>
        /// 设置角色菜单
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public int AddRoleMenu(int roleid, int menuid)
        {
            var rolemenu = new Base_RoleMenuMap();
            rolemenu.RoleID = roleid;
            rolemenu.MenuID = menuid;
            return dbUtil._myDb.InsertIfNotExists(rolemenu, x => x.RoleID == roleid && x.MenuID == menuid);
        }


        /// <summary>
        /// 删除角色菜单关联
        /// </summary>
        /// <param name="roleid">角色id</param>
        /// <param name="menuid">菜单id</param>
        public void DeleteRoleMenu(int roleid,int menuid)
        {
            var m = dbUtil._myDb.Query<Base_RoleMenuMap>().Where(x => x.MenuID == menuid && x.RoleID == roleid).FirstOrDefault();
            if (m != null) 
              dbUtil._myDb.Delete<Base_RoleMenuMap>(m.ID);
        }
    }
}
