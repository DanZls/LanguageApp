using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationAlternativeForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativeForms",
                table: "Translations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativeForms",
                table: "Translations");
        }
    }
}
