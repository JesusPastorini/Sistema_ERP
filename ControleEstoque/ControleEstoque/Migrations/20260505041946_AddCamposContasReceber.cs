using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposContasReceber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cliente_id",
                table: "contas_receber",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "contas_receber",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_contas_receber_cliente_id",
                table: "contas_receber",
                column: "cliente_id");

            migrationBuilder.AddForeignKey(
                name: "fk_contas_receber_clientes_cliente_id",
                table: "contas_receber",
                column: "cliente_id",
                principalTable: "clientes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contas_receber_clientes_cliente_id",
                table: "contas_receber");

            migrationBuilder.DropIndex(
                name: "ix_contas_receber_cliente_id",
                table: "contas_receber");

            migrationBuilder.DropColumn(
                name: "cliente_id",
                table: "contas_receber");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "contas_receber");
        }
    }
}
