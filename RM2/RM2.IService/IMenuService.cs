using RM2.Model;
using RM2.Model.BusinesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.IService
{
    public interface IMenuService
    {
        List<Base_Menu> GetMenuList(PageModel page, out int recordcoun);
        int AddMenu(Base_Menu Menu);
        int UpdateMenu(Base_Menu Menu);
        int DeleteMenu(Base_Menu Menu);
    }
}
