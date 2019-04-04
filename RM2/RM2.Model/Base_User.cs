namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// �û���
    /// </summary>
    public partial class Base_User
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// �û�����
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// ��¼�˻�
        /// </summary>
        [StringLength(50)]
        public string Account { get; set; }
        /// <summary>
        /// ��¼����
        /// </summary>
        [StringLength(50)]
        public string Password { get; set; }
        /// <summary>
        /// ������Կ
        /// </summary>
        [StringLength(50)]
        public string Secretkey { get; set; }
        /// <summary>
        /// ��ʵ����
        /// </summary>
        [StringLength(50)]
        public string RealName { get; set; }
        /// <summary>
        /// �س�
        /// </summary>
        [StringLength(50)]
        public string NickName { get; set; }
        /// <summary>
        /// ͷ��
        /// </summary>
        [StringLength(50)]
        public string HeadIcon { get; set; }
        /// <summary>
        /// �Ա�0Ů1�У�
        /// </summary>
        public int? Sex { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// �ֻ�
        /// </summary>
        [StringLength(50)]
        public string Mobile { get; set; }
        /// <summary>
        /// �����ʼ�
        /// </summary>
        [StringLength(50)]
        public string Email { get; set; }
        /// <summary>
        /// QQ��
        /// </summary>
        [StringLength(50)]
        public string OICQ { get; set; }
        /// <summary>
        /// ΢�ź�
        /// </summary>
        [StringLength(50)]
        public string WeChat { get; set; }
        /// <summary>
        /// ����¼ʱ��
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
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
