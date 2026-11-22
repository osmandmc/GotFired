using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotFired.Model.Entities
{
    /// <summary>
    /// Columns every application table has. Creator/Editor are legacy integer columns that are not populated;
    /// RowVersion is a 16-character string regenerated on every save and used as a concurrency token.
    /// All of these are set by GotFiredDbContext.SaveChanges (through EF Core, hence the private setters).
    /// </summary>
    public abstract class BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; private set; }
        public int Editor { get; private set; }
        public int Creator { get; private set; }
        [Required]
        public DateTime CreatedDate { get; private set; }
        [Required]
        public DateTime EditedDate { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsActive { get; private set; }
        [Required, MaxLength(16)]
        public string RowVersion { get; private set; }
    }
}
