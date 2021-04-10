using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ESD_Project.Models
{
    [Table("FileDetail")]
    public class FileDetail
    {
        [Key]
        public int FileDetailsId { get; set; }

        [StringLength(500)]
        public string FileName { get; set; }

        [StringLength(500)]
        public string ContentType { get; set; }

        public long? PostId { get; set; }

        public long? UserId { get; set; }

        public long? TopicId { get; set; }

        public long? MajorId { get; set; }

        public byte[] Data { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }

        public virtual Topic Topic { get; set; }

        public virtual Major Major { get; set; }
    }
}