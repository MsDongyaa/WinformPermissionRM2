namespace RM2.Model
{
    using MyMiniOrm.Attributes;
    using MyMiniOrm.Commons;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// ��ɫ��ɫӳ���
    /// </summary>
    public partial class Base_UserRoleMap : IEntity
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

        [MyForeignKey("UserID")]
        public Base_User User { get; set; }
        /// <summary>
        /// ��ɫID
        /// </summary>
        public int? RoleID { get; set; }
        [MyForeignKey("RoleID")]
        public Base_Role Role { get; set; }
    }
}
