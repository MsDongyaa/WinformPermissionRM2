namespace RM2.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    /// <summary>
    /// �û��˵�ӳ���
    /// </summary>
    public partial class Base_UserMenuMap
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
        /// �˵�ID
        /// </summary>
        public int? MenuID { get; set; }
    }
}
