using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AutoPlayAudio = table.Column<bool>(type: "bit", nullable: false),
                    AudioLoudness = table.Column<double>(type: "float", nullable: false),
                    Language1Tag = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Language2Tag = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
