using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Añadir una columna obligatoria y unica a una tabla con filas no se puede hacer
            // de una sola vez: las existentes quedarian todas con el mismo valor vacio y el
            // indice unico fallaria. Se hace en tres pasos: nulable, relleno, y ya obligatoria.
            migrationBuilder.AddColumn<string>(
                name: "tax_id", table: "users", type: "character varying(9)",
                maxLength: 9, nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone", table: "users", type: "character varying(9)",
                maxLength: 9, nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code", table: "users", type: "character varying(5)",
                maxLength: 5, nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_account", table: "users", type: "character varying(34)",
                maxLength: 34, nullable: true);

            // Los socios que ya existian no tienen datos de facturacion y no hay forma de
            // inventarselos: en un sistema real habria que pedirselos antes de exigir el
            // campo. Aqui solo hay datos de desarrollo, asi que se les genera un NIF valido
            // distinto a cada uno para no romper el indice unico.
            //
            // La letra de control se saca de la tabla oficial indexando por el resto entre
            // 23, igual que en el codigo; substr empieza en 1, de ahi el +1.
            migrationBuilder.Sql(
                """
                WITH numerados AS (
                    SELECT id, 90000000 + row_number() OVER (ORDER BY created_at) AS n
                    FROM users
                )
                UPDATE users u SET
                    tax_id = lpad(numerados.n::text, 8, '0')
                             || substr('TRWAGMYFPDXBNJZSQVHLCKE', (numerados.n % 23)::int + 1, 1),
                    phone = '600000000',
                    postal_code = '28001'
                FROM numerados
                WHERE u.id = numerados.id;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "tax_id", table: "users", type: "character varying(9)",
                maxLength: 9, nullable: false, oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "phone", table: "users", type: "character varying(9)",
                maxLength: 9, nullable: false, oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "postal_code", table: "users", type: "character varying(5)",
                maxLength: 5, nullable: false, oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_tax_id", table: "users", column: "tax_id", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_tax_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "bank_account",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "users");

            migrationBuilder.DropColumn(
                name: "postal_code",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tax_id",
                table: "users");
        }
    }
}
