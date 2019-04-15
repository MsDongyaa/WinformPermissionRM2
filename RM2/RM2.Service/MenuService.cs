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
    public class MenuService : IMenuService
    {
        /// <summary>
        /// 获取菜单列表数据
        /// </summary>
        /// <param name="page">分页参数</param>
        /// <param name="recordcount">总记录数</param>
        /// <returns></returns>
        public List<Base_Menu> GetMenuList(PageModel page,out int recordcount)
        {
            recordcount = 0;
            var userlist = dbUtil._myDb.PageList<Base_Menu>(page.pageIndex, page.pageIndex, out recordcount, x=>x.DeleteMark!=1, x => x.CreateDate, MyDbSort.Desc);
            return userlist;
        }


        public int AddMenu(Base_Menu user)
        {
            return dbUtil._myDb.Insert(user);
        }



        public int UpdateMenu(Base_Menu user)
        {
            return dbUtil._myDb.Update(user);
        }



        public int DeleteMenu(Base_Menu user)
        {
            user.DeleteMark = 1;
            return dbUtil._myDb.Update(user);
        }
    }
}
