using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camar.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddBlockedDays : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "blocked_days",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_blocked_days", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_blocked_days_date",
            table: "blocked_days",
            column: "date",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "blocked_days");
    }
}
