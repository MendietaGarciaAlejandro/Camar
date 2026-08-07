using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camar.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddNoOverlapConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
    ALTER TABLE reservations
    ADD CONSTRAINT ck_reservations_no_overlap
    EXCLUDE USING gist (
        resource_id WITH =,
        period WITH &&
    )
    WHERE (status = 1);
    """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
    ALTER TABLE reservations
    DROP CONSTRAINT ck_reservations_no_overlap;
    """);
    }
}
