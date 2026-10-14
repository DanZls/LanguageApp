using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationInfoRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TranslationLearningInfoTable_TranslationId",
                table: "TranslationLearningInfoTable",
                column: "TranslationId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationLearningInfoTable_UserId",
                table: "TranslationLearningInfoTable",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationLearningInfoTable_Translations_TranslationId",
                table: "TranslationLearningInfoTable",
                column: "TranslationId",
                principalTable: "Translations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationLearningInfoTable_Users_UserId",
                table: "TranslationLearningInfoTable",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationLearningInfoTable_Translations_TranslationId",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropForeignKey(
                name: "FK_TranslationLearningInfoTable_Users_UserId",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropIndex(
                name: "IX_TranslationLearningInfoTable_TranslationId",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropIndex(
                name: "IX_TranslationLearningInfoTable_UserId",
                table: "TranslationLearningInfoTable");
        }
    }
}
