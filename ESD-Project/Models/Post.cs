namespace ESD_Project.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Post")]
    public partial class Post
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Post()
        {
            Comment = new HashSet<Comment>();
            FileDetails = new HashSet<FileDetail>();
        }
        [Key]
        [Display(Name = "Post Code")]
        public long PostId { get; set; }

        [Display(Name = "Post Title")]
        [StringLength(250)]
        public string Name { get; set; }

        [Display(Name = "Meta Title")]
        [StringLength(250)]
        public string MetaTitle { get; set; }

        [StringLength(500)]
        public string Desciption { get; set; }

        public bool? Status { get; set; }

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

        
        public long? TopicId { get; set; }

        
        public long? MajorId { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual Major Major { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FileDetail> FileDetails { get; set; }
    }
}
