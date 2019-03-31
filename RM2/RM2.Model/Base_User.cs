namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// 用户表
    /// </summary>
    public partial class Base_User
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int ID { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        [StringLength(50)]
        public string EnCode { get; set; }
        /// <summary>
        /// 登录账户
        /// </summary>
        [StringLength(50)]
        public string Account { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        [StringLength(50)]
        public string Password { get; set; }
        /// <summary>
        /// 密码秘钥
        /// </summary>
        [StringLength(50)]
        public string Secretkey { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        [StringLength(50)]
        public string RealName { get; set; }
        /// <summary>
        /// 呢称
        /// </summary>
        [StringLength(50)]
        public string NickName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(50)]
        public string HeadIcon { get; set; }
        /// <summary>
        /// 性别（0女1男）
        /// </summary>
        public int? Sex { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        [StringLength(50)]
        public string Mobile { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        [StringLength(50)]
        public string Email { get; set; }
        /// <summary>
        /// QQ号
        /// </summary>
        [StringLength(50)]
        public string OICQ { get; set; }
        /// <summary>
        /// 微信号
        /// </summary>
        [StringLength(50)]
        public string WeChat { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
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
