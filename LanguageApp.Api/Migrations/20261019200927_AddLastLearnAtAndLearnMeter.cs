using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLearnAtAndLearnMeter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLearnAt",
                table: "TranslationLearningInfoTable",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LearnMeter",
                table: "TranslationLearningInfoTable",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLearnAt",
                table: "TranslationLearningInfoTable");

            migrationBuilder.DropColumn(
                name: "LearnMeter",
                table: "TranslationLearningInfoTable");
        }
    }
}
