namespace ESD_Project.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("User")]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User(){
            Comment = new HashSet<Comment>();
            FileDetails = new HashSet<FileDetail>();
        }
        public long UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The character lenght can't be more than 100.")]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The character lenght can't be more than 100.")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The character lenght can't be more than 50.")]
        public string Password { get; set; }

        public int? Phone { get; set; }

        [StringLength(200, ErrorMessage = "The character lenght can't be more than 200.")]
        public string Address { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        [Display(Name = "Create Date")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "Create By")]
        [StringLength(50, ErrorMessage = "The character lenght can't be more than 50.")]
        public string CreateBy { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        [Display(Name = "Modify Date")]
        public DateTime? ModifyDate { get; set; }

        [Display(Name = "Modify By")]
        [StringLength(50, ErrorMessage = "The character lenght can't be more than 50.")]
        public string ModifyBy { get; set; }

        [Display(Name="Account Status")]
        public bool? AccountStatus { get; set; }

        public string GroupId { get; set; }

        public long? MajorId { get; set; }

        public virtual GroupMember GroupMember { get; set; }

        public virtual Major Major { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FileDetail> FileDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comment { get; set; }
    }
}
