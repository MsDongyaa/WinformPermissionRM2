using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM2.Model.BusinesModel
{
    public class PageModel
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public string keyword { get; set; }
    }
}
