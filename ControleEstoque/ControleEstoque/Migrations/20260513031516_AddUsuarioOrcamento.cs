using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioOrcamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "condicao_pagamento_id",
                table: "orcamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "forma_pagamento",
                table: "orcamentos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "validade_dias",
                table: "orcamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_condicao_pagamento_id",
                table: "orcamentos",
                column: "condicao_pagamento_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orcamentos_condicoes_pagamento_condicao_pagamento_id",
                table: "orcamentos",
                column: "condicao_pagamento_id",
                principalTable: "condicoes_pagamento",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orcamentos_condicoes_pagamento_condicao_pagamento_id",
                table: "orcamentos");

            migrationBuilder.DropIndex(
                name: "ix_orcamentos_condicao_pagamento_id",
                table: "orcamentos");

            migrationBuilder.DropColumn(
                name: "condicao_pagamento_id",
                table: "orcamentos");

            migrationBuilder.DropColumn(
                name: "forma_pagamento",
                table: "orcamentos");

            migrationBuilder.DropColumn(
                name: "validade_dias",
                table: "orcamentos");
        }
    }
}
