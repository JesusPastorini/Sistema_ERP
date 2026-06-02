using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Models
{
    public class EntradaEstoqueItem
    {
        public int Id { get; set; }

        public int EntradaEstoqueId { get; set; }

        public EntradaEstoque EntradaEstoque { get; set; }

        // PRODUTO
        public int ProdutoId { get; set; }

        public Produto Produto { get; set; }

        // DADOS
        public decimal Quantidade { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ValorCusto { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Subtotal { get; set; }
    }
}