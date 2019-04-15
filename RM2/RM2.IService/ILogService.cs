using RM2.Model;
using RM2.Model.BusinesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.IService
{
    public interface ILogService
    {
        List<Base_Log> GetLogList(PageModel page, out int recordcoun);
        int AddLog(Base_Log Log);
        int UpdateLog(Base_Log Log);
        int DeleteLog(Base_Log Log);
    }
}
