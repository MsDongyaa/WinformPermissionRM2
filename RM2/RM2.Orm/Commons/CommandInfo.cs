using System;
using System.Data.SqlClient;

namespace MyMiniOrm.Commons
{
    public enum EffectNextType
    {
        /// <summary>
        /// 对其他语句无任何影响 
        /// </summary>
        None,
        /// <summary>
        /// 当前语句必须为"select count(1) from .."格式，如果存在则继续执行，不存在回滚事务
        /// </summary>
        WhenHaveContinue,
        /// <summary>
        /// 当前语句必须为"select count(1) from .."格式，如果不存在则继续执行，存在回滚事务
        /// </summary>
        WhenNoHaveContinue,
        /// <summary>
        /// 当前语句影响到的行数必须大于0，否则回滚事务
        /// </summary>
        ExecuteEffectRows,
        /// <summary>
        /// 引发事件-当前语句必须为"select count(1) from .."格式，如果不存在则继续执行，存在回滚事务
        /// </summary>
        SolicitationEvent
    }   
    public class CommandInfo
    {
        public object ShareObject = null;

        public object OriginalData = null;

        private event EventHandler SolicitationEventHandler;

        public string CommandText;

        public SqlParameter[] Parameters;

        public EffectNextType EffectNextType = EffectNextType.None;

        public event EventHandler SolicitationEvent
        {
            add => SolicitationEventHandler += value;
            remove => SolicitationEventHandler -= value;
        }

        public void OnSolicitationEvent()
        {
            SolicitationEventHandler?.Invoke(this, new EventArgs());
        }

        public CommandInfo(string sqlText, SqlParameter[] para)
        {
            CommandText = sqlText;
            Parameters = para;
        }
        public CommandInfo(string sqlText, SqlParameter[] para, EffectNextType type)
        {
            CommandText = sqlText;
            Parameters = para;
            EffectNextType = type;
        }
    }
}
