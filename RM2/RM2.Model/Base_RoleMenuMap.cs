namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// ��ɫ�˵�ӳ���
    /// </summary>
    public partial class Base_RoleMenuMap
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// �˵�ID
        /// </summary>
        public int? MenuID { get; set; }
        /// <summary>
        /// ��ɫID
        /// </summary>
        public int? RoleID { get; set; }
    }
}
