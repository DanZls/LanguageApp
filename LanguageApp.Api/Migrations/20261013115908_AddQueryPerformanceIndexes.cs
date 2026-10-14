using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TranslationLearningInfoTable_UserId",
                table: "TranslationLearningInfoTable");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Language2Tag",
                table: "Translations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language1Tag",
                table: "Translations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_Language1Tag_Language2Tag_FrequencyIndex",
                table: "Translations",
                columns: new[] { "Language1Tag", "Language2Tag", "FrequencyIndex" },
                filter: "[IsSkipped] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationLearningInfoTable_UserId_TranslationId",
                table: "TranslationLearningInfoTable",
                columns: new[] { "UserId", "TranslationId" })
                .Annotation("SqlServer:Include", new[] { "FirstLearnAt", "SecondLearnAt", "ThirdLearnAt", "LastViewAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Translations_Language1Tag_Language2Tag_FrequencyIndex",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_TranslationLearningInfoTable_UserId_TranslationId",
                table: "TranslationLearningInfoTable");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Language2Tag",
                table: "Translations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language1Tag",
                table: "Translations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationLearningInfoTable_UserId",
                table: "TranslationLearningInfoTable",
                column: "UserId");
        }
    }
}
