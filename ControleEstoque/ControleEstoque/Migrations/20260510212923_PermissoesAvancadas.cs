using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class PermissoesAvancadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "pode_gerenciar_clientes",
                table: "permissoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pode_gerenciar_estoque",
                table: "permissoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pode_gerenciar_produtos",
                table: "permissoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pode_gerenciar_vendas",
                table: "permissoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pode_ver_financeiro",
                table: "permissoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pode_gerenciar_clientes",
                table: "permissoes");

            migrationBuilder.DropColumn(
                name: "pode_gerenciar_estoque",
                table: "permissoes");

            migrationBuilder.DropColumn(
                name: "pode_gerenciar_produtos",
                table: "permissoes");

            migrationBuilder.DropColumn(
                name: "pode_gerenciar_vendas",
                table: "permissoes");

            migrationBuilder.DropColumn(
                name: "pode_ver_financeiro",
                table: "permissoes");
        }
    }
}
