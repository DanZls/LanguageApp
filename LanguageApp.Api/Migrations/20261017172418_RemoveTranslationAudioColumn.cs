using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTranslationAudioColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Abort instead of losing audio that was never copied to Pronunciations
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM Translations t
                    WHERE t.TermTranslationAudio IS NOT NULL
                      AND NOT EXISTS (
                          SELECT 1 FROM Pronunciations p
                          WHERE p.TranslationId = t.Id AND p.Pronunciation = t.TermTranslationAudio))
                    THROW 50000, 'Translations audio is not fully copied to Pronunciations', 1;");

            migrationBuilder.DropColumn(
                name: "TermTranslationAudio",
                table: "Translations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "TermTranslationAudio",
                table: "Translations",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE t SET t.TermTranslationAudio = p.Pronunciation
                FROM Translations t
                JOIN Pronunciations p ON p.TranslationId = t.Id");
        }
    }
}
