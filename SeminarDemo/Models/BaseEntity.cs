using System.ComponentModel.DataAnnotations;

namespace SeminarDemo.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
