using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPrixToLivre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Prix",
                table: "Livres",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Livres");
        }
    }
}
