namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// ��ɫ��ɫӳ���
    /// </summary>
    public partial class Base_UserRoleMap
    {
        /// <summary>
        /// ����ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// �û�ID
        /// </summary>
        public int? UserID { get; set; }
        /// <summary>
        /// ��ɫID
        /// </summary>
        public int? RoleID { get; set; }
    }
}
