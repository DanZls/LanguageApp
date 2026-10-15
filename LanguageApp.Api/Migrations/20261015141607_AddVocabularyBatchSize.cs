using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyBatchSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VocabularyBatchSize",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VocabularyBatchSize",
                table: "UserSettings");
        }
    }
}
