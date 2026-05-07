using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleEstoque.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposContasReceberModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contas_receber_vendas_venda_id",
                table: "contas_receber");

            migrationBuilder.AlterColumn<int>(
                name: "venda_id",
                table: "contas_receber",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_contas_receber_vendas_venda_id",
                table: "contas_receber",
                column: "venda_id",
                principalTable: "vendas",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_contas_receber_vendas_venda_id",
                table: "contas_receber");

            migrationBuilder.AlterColumn<int>(
                name: "venda_id",
                table: "contas_receber",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_contas_receber_vendas_venda_id",
                table: "contas_receber",
                column: "venda_id",
                principalTable: "vendas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
