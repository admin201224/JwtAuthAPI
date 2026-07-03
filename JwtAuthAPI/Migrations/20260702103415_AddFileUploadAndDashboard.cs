using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUploadAndDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseContents_CourseId_OrderIndex",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "IsPreview",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "CourseContents");

            migrationBuilder.RenameColumn(
                name: "OrderIndex",
                table: "CourseContents",
                newName: "DownloadCount");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseContents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CourseContents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CourseContents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "CourseContents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseContents_CourseId",
                table: "CourseContents",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId",
                table: "CourseEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_UserId",
                table: "CourseEnrollments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseContents_CourseId",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "CourseContents");

            migrationBuilder.RenameColumn(
                name: "DownloadCount",
                table: "CourseContents",
                newName: "OrderIndex");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseContents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "CourseContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreview",
                table: "CourseContents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "CourseContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseContents_CourseId_OrderIndex",
                table: "CourseContents",
                columns: new[] { "CourseId", "OrderIndex" });
        }
    }
}
