namespace RM2.Model
{
    using MyMiniOrm.Commons;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// �˵���
    /// </summary>
    public partial class Base_Menu : IEntity
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// �����˵�ID
        /// </summary>
        public int? ParentID { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        [StringLength(50)]
        public string MenuName { get; set; }
        /// <summary>
        /// ͼ��
        /// </summary>
        [StringLength(50)]
        public string Icon { get; set; }
        /// <summary>
        /// ������ַ
        /// </summary>
        [StringLength(200)]
        public string UrlAddress { get; set; }
        /// <summary>
        /// �˵��ȼ�
        /// </summary>
        public int? Level { get; set; }
        /// <summary>
        /// �˵�·��
        /// </summary>
        [StringLength(1000)]
        public string Path { get; set; }
        /// <summary>
        /// �˵�����
        /// </summary>
        public int? Type { get; set; }
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
