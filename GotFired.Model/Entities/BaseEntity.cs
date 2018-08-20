using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace GotFired.Model.Entities
{
    public interface IRowVersion
    {
        string RowVersion { get; set; }
    }
    public interface IBaseEntity : IRowVersion
    {

        int ID { get; set; }

        int Editor { get; set; }
        int Creator { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime EditedDate { get; set; }
    }

    public abstract class BaseEntity : IBaseEntity
    {
        [DebuggerHidden]
        //public BaseEntity()
        //{
        //    Creator = 1;  // master/super UserId
        //    Editor = 1;   // master/super UserId
        //    IsActive = true;
        //}
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get; set; }

        public virtual int Editor { get; set; }
        public virtual int Creator { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime EditedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        [Required, MaxLength(16)]
        public string RowVersion { get; set; }
        //public byte[] RowVersion { get; set; }

        //[NotMapped]
        // If you on .NET 4.5.2 or up,dont use 
        //public virtual int TimeStamp
        //{
        //get { return RowVersion == 0 ? 0 : Convert.ToChar(RowVersion); }
        //set { RowVersion = value == 0 ? 0 : Convert.ToInt32(value); }
        //get { return Convert.ToChar(RowVersion); }
        //set { RowVersion = Convert.ToInt32(value); }
        //}
    }

    //public class EntityBase
    //{
    //    public int ID { get; set; }
    //    //[Timestamp]
    //    //public byte[] RowVersion { get; set; }
    //    //[Display(Name = "CreateDate", ResourceType = typeof(Resources))]
    //    public DateTime CreateDate { get; set; }
    //    //[Display(Name = "LastModifyDate", ResourceType = typeof(Resources))]
    //    public DateTime LastModifyDate { get; set; }
    //    //[Display(Name = "CreateUserName", ResourceType = typeof(Resources))]
    //    [StringLength(63)]
    //    public string CreateUserName { get; set; }
    //    //[Display(Name = "LastModifyUserName", ResourceType = typeof(Resources))] 
    //    [StringLength(63)]
    //    public string LastModifyUserName { get; set; }
    //    [NotMapped]
    //    public bool MarkedForDelete { get; set; }
    //    [NotMapped]
    //    public string IdEncrypted { get; set; }
    //    //[NotMapped]
    //    //public List<string> ChangedProperties{ get; set; }
    //    ////Override this when you need to show this entity in a combo box, this is a viewmodel property so do not map this on a db column
    //    //[NotMapped]
    //    //public virtual string Caption { get; }
    //    ////Set this to true when you need to show this entity in a dialog box, this is a viewmodel property so do not map this on a db column
    //    //[NotMapped]
    //    //public string IsDisplayedInsideDialog { get; set; }

    //    //[Display(Name = "CreatedUser", ResourceType = typeof(Resources))]
    //    //public int? CreateApplicationUserId { get; set; }
    //    //[Display(Name = "LastModifiedUser", ResourceType = typeof(Resources))]
    //    //public int? LastModifyApplicationUserId { get; set; }
    //    //        [ForeignKey("Id")]
    //    //public virtual ApplicationUser ApplicationUser { get; set; }


    //}
}
