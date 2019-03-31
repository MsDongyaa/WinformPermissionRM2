namespace RM2.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelDBContext : DbContext
    {
        public ModelDBContext()
            : base("name=ModelDBContext")
        {
        }

        public virtual DbSet<Base_Log> Base_Log { get; set; }
        public virtual DbSet<Base_Menu> Base_Menu { get; set; }
        public virtual DbSet<Base_Role> Base_Role { get; set; }
        public virtual DbSet<Base_RoleMenuMap> Base_RoleMenuMap { get; set; }
        public virtual DbSet<Base_User> Base_User { get; set; }
        public virtual DbSet<Base_UserMenuMap> Base_UserMenuMap { get; set; }
        public virtual DbSet<Base_UserRoleMap> Base_UserRoleMap { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Base_Log>()
                .Property(e => e.SourceObject)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Log>()
                .Property(e => e.SourceContentJson)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Log>()
                .Property(e => e.OperateAccount)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Menu>()
                .Property(e => e.EnCode)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Menu>()
                .Property(e => e.Icon)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Menu>()
                .Property(e => e.UrlAddress)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Menu>()
                .Property(e => e.Path)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Role>()
                .Property(e => e.EnCode)
                .IsUnicode(false);

            modelBuilder.Entity<Base_Role>()
                .Property(e => e.FullName)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.EnCode)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.Secretkey)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.HeadIcon)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.Mobile)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.OICQ)
                .IsUnicode(false);

            modelBuilder.Entity<Base_User>()
                .Property(e => e.WeChat)
                .IsUnicode(false);
        }
    }
}
