using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class RelacaoVendaMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "movimentacoes_estoque",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "venda_id",
                table: "movimentacoes_estoque",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_venda_id",
                table: "movimentacoes_estoque",
                column: "venda_id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_estoque_vendas_venda_id",
                table: "movimentacoes_estoque",
                column: "venda_id",
                principalTable: "vendas",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_estoque_vendas_venda_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoes_estoque_venda_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "movimentacoes_estoque");

            migrationBuilder.DropColumn(
                name: "venda_id",
                table: "movimentacoes_estoque");
        }
    }
}
