using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class InsertSuperAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = N'227b357b6fdb415d8780aa6bf5272080')
                BEGIN
                    INSERT INTO dbo.Users (Id, IsSuperAdmin, FirstName, MiddleName, LastName, EmailAddress, PasswordHash,
                                           ImageUrl, IsApproved, CreateTime, LastModifiedTime, CreatedBy, LastModifiedBy,
                                           IsDeleted, RoleId, UserName, IsActive, Address, GoBangladeshStatus, BloodGroup,
                                           DateOfBirth, District, FatherName, FullName, Gender, LastDonationTime, MobileNumber,
                                           MotherName, [Union], Upazila, UserType, GoBangladeshCount, Dob, NidUrls,
                                           PhysicalComplexity, Code, Serial, InstituteName, LeaderType)
                    VALUES (N'227b357b6fdb415d8780aa6bf5272080', 1, null, null, null, null,
                            N'$2a$12$QtUQXdr3FCCHz7WtKA.DmOp6wU6MLopN/gCFxMjecoSh8fk0bExrm', N'', 1, N'2025-03-15 14:13:46.6998195',
                            N'2025-03-15 14:13:46.6998195', N'', N'', 0, null, null, 1, null, null, N'O+', N'1998-05-08', N'1', null,
                            N'Mehrab Hosen Mahi', N'Male', null, N'01521323545', null, N'0b4d7206d4fb4c6bb647bfecd081ef6d',
                            N'1a7f59315d5046309b719da888cd5f18', N'Admin', 0, N'1998-05-08 00:00:00.0000000', N'', N'0', N'000001', 1, null,
                            null);
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
