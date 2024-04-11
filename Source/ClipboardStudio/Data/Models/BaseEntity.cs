using System;
using System.ComponentModel.DataAnnotations;

namespace ClipboardStudio.Data.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public DateTime ModifyDateUtc { get; set; }
    }
}
