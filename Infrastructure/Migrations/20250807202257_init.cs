using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduShpere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appUsers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<int>(type: "nvarchar(max)", nullable: false),
                    PassWord = table.Column<int>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<int>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<int>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<int>(type: "nvarchar(max)", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    isBanned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appUsers", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appUsers");
        }
    }
}
