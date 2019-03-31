namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// 角色表
    /// </summary>
    public partial class Base_Role
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 角色编码
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        [StringLength(50)]
        public string FullName { get; set; }
        /// <summary>
        /// 公共角色
        /// </summary>
        public int? IsPublic { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? OverdueTime { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 删除标记
        /// </summary>
        public int? DeleteMark { get; set; }
        /// <summary>
        /// 有效标志
        /// </summary>
        public int? EnabledMark { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 创建用户主键
        /// </summary>
        public int? CreateUserId { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        [StringLength(50)]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifyDate { get; set; }
        /// <summary>
        /// 修改用户主键
        /// </summary>
        public int? ModifyUserId { get; set; }
        /// <summary>
        /// 修改用户
        /// </summary>
        [StringLength(50)]
        public string ModifyUserName { get; set; }
    }
}
