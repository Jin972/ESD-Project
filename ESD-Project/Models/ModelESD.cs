using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ESD_Project.Models
{
    public partial class ModelESD : DbContext
    {
        public ModelESD()
            : base("name=ModelESD")
        {
        }

        public virtual DbSet<AcademicYear> AcademicYears { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<GroupMember> GroupMembers { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Credential> Credentials { get; set; }
        public virtual DbSet<FileDetail> FileDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<GroupMember>()
                .Property(e => e.GroupId)
                .IsUnicode(false);

            modelBuilder.Entity<Major>()
                .Property(e => e.MetaTitle)
                .IsUnicode(false);

            modelBuilder.Entity<Post>()
                .Property(e => e.MetaTitle)
                .IsUnicode(false);

            modelBuilder.Entity<Role>()
                .Property(e => e.RoleId)
                .IsUnicode(false);

            modelBuilder.Entity<Topic>()
                .Property(e => e.MetaTitle)
                .IsUnicode(false);
        }
    }
}
