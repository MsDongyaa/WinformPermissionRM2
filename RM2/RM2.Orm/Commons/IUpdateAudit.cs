using System;

namespace RM2.Orm.Commons
{
    public interface IUpdateAudit
    {
        string Updator { get; set; }

        DateTime UpdateAt { get; set; }
    }
}
