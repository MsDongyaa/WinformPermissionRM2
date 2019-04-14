using RM2.Model;
using RM2.Model.BusinesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.IService
{
    public interface IRoleService
    {
        List<Base_Role> GetRoleList(PageModel page, out int recordcoun);
        int AddRole(Base_Role role);
        int UpdateRole(Base_Role role);
        int DeleteRole(Base_Role role);
    }
}
