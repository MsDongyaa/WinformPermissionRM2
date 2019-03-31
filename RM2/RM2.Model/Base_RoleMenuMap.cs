namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// 角色菜单映射表
    /// </summary>
    public partial class Base_RoleMenuMap
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public int? MenuID { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public int? RoleID { get; set; }
    }
}
