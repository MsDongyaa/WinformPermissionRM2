using System;

namespace MyMiniOrm.Commons
{
    public interface ICreateAudit
    {
        DateTime CreateAt { get; set; }

        string Creator { get; set; }
    }
}
