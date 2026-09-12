using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StayHub.Dal.Data;

#nullable disable

namespace StayHub.Dal.Migrations;

[DbContext(typeof(StayHubDbContext))]
[Migration("20260911120000_AddSynchronizationRuns")]
public partial class AddSynchronizationRuns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SynchronizationRuns",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SourceId = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedCount = table.Column<int>(type: "int", nullable: false),
                UpdatedCount = table.Column<int>(type: "int", nullable: false),
                UnchangedCount = table.Column<int>(type: "int", nullable: false),
                ConflictCount = table.Column<int>(type: "int", nullable: false),
                FailedCount = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SynchronizationRuns", x => x.Id);
                table.ForeignKey(
                    name: "FK_SynchronizationRuns_Sources_SourceId",
                    column: x => x.SourceId,
                    principalTable: "Sources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SynchronizationRuns_SourceId_StartedAtUtc",
            table: "SynchronizationRuns",
            columns: new[] { "SourceId", "StartedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SynchronizationRuns");
    }
}
