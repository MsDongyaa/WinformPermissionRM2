namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// 系统日志表
    /// </summary>
    public partial class Base_Log
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 分类Id 1-登陆2-访问3-异常
        /// </summary>
        public int? CategoryId { get; set; }
        /// <summary>
        /// 来源对象
        /// </summary>
        [StringLength(200)]
        public string SourceObject { get; set; }
        /// <summary>
        /// 来源日志内容
        /// </summary>
        [Column(TypeName = "text")]
        public string SourceContentJson { get; set; }
        /// <summary>
        /// 删除标记
        /// </summary>
        public DateTime? OperateTime { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public int? OperateUserId { get; set; }
        /// <summary>
        /// 操作用户Id
        /// </summary>
        [StringLength(50)]
        public string OperateAccount { get; set; }
        /// <summary>
        /// 菜单ID
        /// </summary>
        public int? MenuId { get; set; }
        /// <summary>
        /// 菜单
        /// </summary>
        [StringLength(50)]
        public string Menu { get; set; }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public int? MenuType { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }
        /// <summary>
        /// 删除标记
        /// </summary>
        public int? DeleteMark { get; set; }
        /// <summary>
        /// 有效标志
        /// </summary>
        public int? EnabledMark { get; set; }
    }
}
