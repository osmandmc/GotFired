using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotFired.Model.Entities
{
    /// <summary>
    /// Columns every application table has. Creator/Editor are legacy integer columns that are not populated;
    /// RowVersion is a 16-character string regenerated on every save and used as a concurrency token.
    /// The audit fields are stamped by GotFiredDbContext.SaveChanges.
    /// </summary>
    public abstract class BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int Editor { get; set; }
        public int Creator { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime EditedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        [Required, MaxLength(16)]
        public string RowVersion { get; set; }
    }
}
