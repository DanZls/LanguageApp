using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pronunciations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranslationId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageTag = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Pronunciation = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pronunciations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pronunciations_Translations_TranslationId",
                        column: x => x.TranslationId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pronunciations_TranslationId",
                table: "Pronunciations",
                column: "TranslationId",
                unique: true);

            // Copy existing audio; Translations is left untouched
            migrationBuilder.Sql(@"
                INSERT INTO Pronunciations (TranslationId, Term, LanguageTag, Pronunciation)
                SELECT Id, TermTranslation, ISNULL(Language2Tag, ''), TermTranslationAudio
                FROM Translations
                WHERE TermTranslationAudio IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pronunciations");
        }
    }
}
