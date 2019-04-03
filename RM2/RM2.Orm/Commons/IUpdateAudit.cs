using System;

namespace MyMiniOrm.Commons
{
    public interface IUpdateAudit
    {
        string Updator { get; set; }

        DateTime UpdateAt { get; set; }
    }
}
