using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayHub.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_ExternalId",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE r
                SET r.SourceId = s.Id
                FROM Reservations AS r
                CROSS JOIN (
                    SELECT TOP (1) Id
                    FROM Sources
                    WHERE Name = N'Manual Import'
                    ORDER BY Id
                ) AS s;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "SourceId",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SourceId_ExternalId",
                table: "Reservations",
                columns: new[] { "SourceId", "ExternalId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Sources_SourceId",
                table: "Reservations",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Sources_SourceId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_SourceId_ExternalId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ExternalId",
                table: "Reservations",
                column: "ExternalId",
                unique: true);
        }
    }
}
