using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaMovimentacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "produto_destino_id",
                table: "movimentacoes_estoque",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario",
                table: "movimentacoes_estoque",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_estoque_produto_destino_id",
                table: "movimentacoes_estoque",
                column: "produto_destino_id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_estoque_produtos_produto_destino_id",
                table: "movimentacoes_estoque",
                column: "produto_destino_id",
                principalTable: "produtos",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_estoque_produtos_produto_destino_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoes_estoque_produto_destino_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropColumn(
                name: "produto_destino_id",
                table: "movimentacoes_estoque");

            migrationBuilder.DropColumn(
                name: "valor_unitario",
                table: "movimentacoes_estoque");
        }
    }
}
