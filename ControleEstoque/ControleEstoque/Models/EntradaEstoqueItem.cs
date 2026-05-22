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

        public decimal ValorCusto { get; set; }

        public decimal Subtotal { get; set; }
    }
}