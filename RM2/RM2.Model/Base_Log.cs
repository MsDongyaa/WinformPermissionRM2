namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// ϵͳ��־��
    /// </summary>
    public partial class Base_Log
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// ����Id 1-��½2-����3-�쳣
        /// </summary>
        public int? CategoryId { get; set; }
        /// <summary>
        /// ��Դ����
        /// </summary>
        [StringLength(200)]
        public string SourceObject { get; set; }
        /// <summary>
        /// ��Դ��־����
        /// </summary>
        [Column(TypeName = "text")]
        public string SourceContentJson { get; set; }
        /// <summary>
        /// ɾ�����
        /// </summary>
        public DateTime? OperateTime { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public int? OperateUserId { get; set; }
        /// <summary>
        /// �����û�Id
        /// </summary>
        [StringLength(50)]
        public string OperateAccount { get; set; }
        /// <summary>
        /// �˵�ID
        /// </summary>
        public int? MenuId { get; set; }
        /// <summary>
        /// �˵�
        /// </summary>
        [StringLength(50)]
        public string Menu { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        public int? MenuType { get; set; }
        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }
        /// <summary>
        /// ɾ�����
        /// </summary>
        public int? DeleteMark { get; set; }
        /// <summary>
        /// ��Ч��־
        /// </summary>
        public int? EnabledMark { get; set; }
    }
}
