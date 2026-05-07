using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeClienteEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cidade",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "complemento",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rua",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uf",
                table: "clientes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bairro",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cidade",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "complemento",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "numero",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "rua",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "uf",
                table: "clientes");
        }
    }
}
