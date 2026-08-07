using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camar.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRefundAmount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "refund_amount",
            table: "reservations",
            type: "numeric(10,2)",
            precision: 10,
            scale: 2,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "refund_amount",
            table: "reservations");
    }
}
