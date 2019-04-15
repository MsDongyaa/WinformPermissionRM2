using RM2.Model;
using RM2.Model.BusinesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.IService
{
    public interface IUserService
    {
        List<Base_User> GetUserList(PageModel page, out int recordcoun);
        int AddUser(Base_User user);
        int UpdateUser(Base_User user);
        int DeleteUser(Base_User user);
    }
}
