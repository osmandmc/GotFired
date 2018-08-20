namespace GotFired.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sgk : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AnswerTemplates",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Content = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Applicants",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AgeInterval = c.Byte(),
                        EducationalState = c.Byte(),
                        Gender = c.Byte(nullable: false),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        FullName = c.String(),
                        PositionId = c.Int(),
                        Creator = c.Int(nullable: false),
                        Editor = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DismissalCaseID = c.Int(nullable: false),
                        Text = c.String(),
                        UserName = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DismissalCases", t => t.DismissalCaseID, cascadeDelete: true)
                .Index(t => t.DismissalCaseID);
            
            CreateTable(
                "dbo.CompanySectors",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.DeclaredTerminationReasons",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.DismissalCases",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Guid = c.String(),
                        ApplicantId = c.Int(nullable: false),
                        CityID = c.Int(nullable: false),
                        EmploymentTerminationDate = c.DateTime(),
                        CompanySectorID = c.Int(),
                        DeclaredTerminationReasonID = c.Int(),
                        OtherDeclaredTerminationReason = c.String(),
                        CategoryID = c.Int(),
                        EmploymentDurationSince = c.Byte(nullable: false),
                        EmployeeCount = c.Byte(nullable: false),
                        CompanySectorName = c.String(),
                        CompanyDescription = c.String(),
                        HasSignedAnyDocument = c.Boolean(nullable: false),
                        SignedDocuments = c.String(),
                        DismissalState = c.Byte(nullable: false),
                        WantShareHisExperience = c.Boolean(nullable: false),
                        DetailedExperience = c.String(),
                        AdditionalInfo = c.String(),
                        AppealState = c.Byte(nullable: false),
                        Notes = c.String(),
                        SGKTerminationReasonID = c.Int(),
                        Creator = c.Int(nullable: false),
                        Editor = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Applicants", t => t.ApplicantId, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .ForeignKey("dbo.Cities", t => t.CityID, cascadeDelete: true)
                .ForeignKey("dbo.CompanySectors", t => t.CompanySectorID)
                .ForeignKey("dbo.DeclaredTerminationReasons", t => t.DeclaredTerminationReasonID)
                .ForeignKey("dbo.SGKTerminationReasons", t => t.SGKTerminationReasonID)
                .Index(t => t.ApplicantId)
                .Index(t => t.CityID)
                .Index(t => t.CompanySectorID)
                .Index(t => t.DeclaredTerminationReasonID)
                .Index(t => t.CategoryID)
                .Index(t => t.SGKTerminationReasonID);
            
            CreateTable(
                "dbo.DismissalCaseSupprtedBies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DismissalCaseId = c.Int(nullable: false),
                        SupportedById = c.Int(nullable: false),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DismissalCases", t => t.DismissalCaseId, cascadeDelete: true)
                .ForeignKey("dbo.SupportedBies", t => t.SupportedById, cascadeDelete: true)
                .Index(t => t.DismissalCaseId)
                .Index(t => t.SupportedById);
            
            CreateTable(
                "dbo.SupportedBies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Creator = c.Int(nullable: false),
                        Editor = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.DismissalCaseTags",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DismissalCaseID = c.Int(nullable: false),
                        TagID = c.Int(nullable: false),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DismissalCases", t => t.DismissalCaseID, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagID, cascadeDelete: true)
                .Index(t => t.DismissalCaseID)
                .Index(t => t.TagID);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.SGKTerminationReasons",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        Editor = c.Int(nullable: false),
                        Creator = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        EditedDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RowVersion = c.String(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FriendlyName = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.DismissalCases", "SGKTerminationReasonID", "dbo.SGKTerminationReasons");
            DropForeignKey("dbo.DismissalCaseTags", "TagID", "dbo.Tags");
            DropForeignKey("dbo.DismissalCaseTags", "DismissalCaseID", "dbo.DismissalCases");
            DropForeignKey("dbo.DismissalCaseSupprtedBies", "SupportedById", "dbo.SupportedBies");
            DropForeignKey("dbo.DismissalCaseSupprtedBies", "DismissalCaseId", "dbo.DismissalCases");
            DropForeignKey("dbo.DismissalCases", "DeclaredTerminationReasonID", "dbo.DeclaredTerminationReasons");
            DropForeignKey("dbo.DismissalCases", "CompanySectorID", "dbo.CompanySectors");
            DropForeignKey("dbo.Comments", "DismissalCaseID", "dbo.DismissalCases");
            DropForeignKey("dbo.DismissalCases", "CityID", "dbo.Cities");
            DropForeignKey("dbo.DismissalCases", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.DismissalCases", "ApplicantId", "dbo.Applicants");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.DismissalCaseTags", new[] { "TagID" });
            DropIndex("dbo.DismissalCaseTags", new[] { "DismissalCaseID" });
            DropIndex("dbo.DismissalCaseSupprtedBies", new[] { "SupportedById" });
            DropIndex("dbo.DismissalCaseSupprtedBies", new[] { "DismissalCaseId" });
            DropIndex("dbo.DismissalCases", new[] { "SGKTerminationReasonID" });
            DropIndex("dbo.DismissalCases", new[] { "CategoryID" });
            DropIndex("dbo.DismissalCases", new[] { "DeclaredTerminationReasonID" });
            DropIndex("dbo.DismissalCases", new[] { "CompanySectorID" });
            DropIndex("dbo.DismissalCases", new[] { "CityID" });
            DropIndex("dbo.DismissalCases", new[] { "ApplicantId" });
            DropIndex("dbo.Comments", new[] { "DismissalCaseID" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.SGKTerminationReasons");
            DropTable("dbo.Tags");
            DropTable("dbo.DismissalCaseTags");
            DropTable("dbo.SupportedBies");
            DropTable("dbo.DismissalCaseSupprtedBies");
            DropTable("dbo.DismissalCases");
            DropTable("dbo.DeclaredTerminationReasons");
            DropTable("dbo.CompanySectors");
            DropTable("dbo.Comments");
            DropTable("dbo.Cities");
            DropTable("dbo.Categories");
            DropTable("dbo.Applicants");
            DropTable("dbo.AnswerTemplates");
        }
    }
}
