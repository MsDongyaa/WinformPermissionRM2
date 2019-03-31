namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// ��ɫ��
    /// </summary>
    public partial class Base_Role
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// ��ɫ����
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// ��ɫ����
        /// </summary>
        [StringLength(50)]
        public string FullName { get; set; }
        /// <summary>
        /// ������ɫ
        /// </summary>
        public int? IsPublic { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? OverdueTime { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// ɾ�����
        /// </summary>
        public int? DeleteMark { get; set; }
        /// <summary>
        /// ��Ч��־
        /// </summary>
        public int? EnabledMark { get; set; }
        /// <summary>
        /// ��ע
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// �����û�����
        /// </summary>
        public int? CreateUserId { get; set; }
        /// <summary>
        /// �����û�
        /// </summary>
        [StringLength(50)]
        public string CreateUserName { get; set; }
        /// <summary>
        /// �޸�����
        /// </summary>
        public DateTime? ModifyDate { get; set; }
        /// <summary>
        /// �޸��û�����
        /// </summary>
        public int? ModifyUserId { get; set; }
        /// <summary>
        /// �޸��û�
        /// </summary>
        [StringLength(50)]
        public string ModifyUserName { get; set; }
    }
}
