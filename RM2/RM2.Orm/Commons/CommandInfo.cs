using System;
using System.Data.SqlClient;

namespace MyMiniOrm.Commons
{
    public enum EffectNextType
    {
        /// <summary>
        /// ������������κ�Ӱ�� 
        /// </summary>
        None,
        /// <summary>
        /// ��ǰ������Ϊ"select count(1) from .."��ʽ��������������ִ�У������ڻع�����
        /// </summary>
        WhenHaveContinue,
        /// <summary>
        /// ��ǰ������Ϊ"select count(1) from .."��ʽ����������������ִ�У����ڻع�����
        /// </summary>
        WhenNoHaveContinue,
        /// <summary>
        /// ��ǰ���Ӱ�쵽�������������0������ع�����
        /// </summary>
        ExecuteEffectRows,
        /// <summary>
        /// �����¼�-��ǰ������Ϊ"select count(1) from .."��ʽ����������������ִ�У����ڻع�����
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
