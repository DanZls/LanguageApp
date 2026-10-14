using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddDatesToTranslationInfoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstLearnDateTime",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "SecondLearnDateTime",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "ThirdLearnDateTime",
                table: "TranslationLearningInfoTable");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstLearnAt",
                table: "TranslationLearningInfoTable",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSkipAt",
                table: "TranslationLearningInfoTable",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SecondLearnAt",
                table: "TranslationLearningInfoTable",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ThirdLearnAt",
                table: "TranslationLearningInfoTable",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstLearnAt",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "LastSkipAt",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "SecondLearnAt",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "ThirdLearnAt",
                table: "TranslationLearningInfoTable");

            migrationBuilder.AddColumn<string>(
                name: "FirstLearnDateTime",
                table: "TranslationLearningInfoTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondLearnDateTime",
                table: "TranslationLearningInfoTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdLearnDateTime",
                table: "TranslationLearningInfoTable",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
