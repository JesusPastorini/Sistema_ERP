using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class CriandoEntradaEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entradas_estoque",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_entrada = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    numero_nota_fiscal = table.Column<string>(type: "text", nullable: true),
                    tipo_entrada = table.Column<string>(type: "text", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    fornecedor_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entradas_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_entradas_estoque_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "entrada_estoque_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrada_estoque_id = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_custo = table.Column<decimal>(type: "numeric", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrada_estoque_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrada_estoque_itens_entradas_estoque_entrada_estoque_id",
                        column: x => x.entrada_estoque_id,
                        principalTable: "entradas_estoque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrada_estoque_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entrada_estoque_itens_entrada_estoque_id",
                table: "entrada_estoque_itens",
                column: "entrada_estoque_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_estoque_itens_produto_id",
                table: "entrada_estoque_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_entradas_estoque_fornecedor_id",
                table: "entradas_estoque",
                column: "fornecedor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entrada_estoque_itens");

            migrationBuilder.DropTable(
                name: "entradas_estoque");
        }
    }
}
