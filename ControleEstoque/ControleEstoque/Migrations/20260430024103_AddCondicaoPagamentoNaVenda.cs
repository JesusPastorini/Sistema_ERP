using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AddCondicaoPagamentoNaVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "condicao_pagamento_id",
                table: "vendas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "condicoes_pagamento",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    parcelas = table.Column<int>(type: "integer", nullable: false),
                    juros = table.Column<decimal>(type: "numeric", nullable: false),
                    dias_recebimento = table.Column<int>(type: "integer", nullable: false),
                    taxa_operadora = table.Column<decimal>(type: "numeric", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_condicoes_pagamento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vendas_condicao_pagamento_id",
                table: "vendas",
                column: "condicao_pagamento_id");

            migrationBuilder.AddForeignKey(
                name: "fk_vendas_condicoes_pagamento_condicao_pagamento_id",
                table: "vendas",
                column: "condicao_pagamento_id",
                principalTable: "condicoes_pagamento",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_vendas_condicoes_pagamento_condicao_pagamento_id",
                table: "vendas");

            migrationBuilder.DropTable(
                name: "condicoes_pagamento");

            migrationBuilder.DropIndex(
                name: "ix_vendas_condicao_pagamento_id",
                table: "vendas");

            migrationBuilder.DropColumn(
                name: "condicao_pagamento_id",
                table: "vendas");
        }
    }
}
