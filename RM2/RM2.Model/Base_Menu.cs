namespace RM2.Model
{
    using MyMiniOrm.Commons;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// 菜单表
    /// </summary>
    public partial class Base_Menu : IEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public int? ParentID { get; set; }
        /// <summary>
        /// 菜单编码
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        [StringLength(50)]
        public string MenuName { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        [StringLength(50)]
        public string Icon { get; set; }
        /// <summary>
        /// 导航地址
        /// </summary>
        [StringLength(200)]
        public string UrlAddress { get; set; }
        /// <summary>
        /// 菜单等级
        /// </summary>
        public int? Level { get; set; }
        /// <summary>
        /// 菜单路径
        /// </summary>
        [StringLength(1000)]
        public string Path { get; set; }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public int? Type { get; set; }
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
