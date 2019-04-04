using System;

namespace RM2.Orm.Commons
{
    public interface ICreateAudit
    {
        DateTime CreateAt { get; set; }

        string Creator { get; set; }
    }
}
