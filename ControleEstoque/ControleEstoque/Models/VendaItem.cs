namespace ControleEstoque.Models
{
    public class VendaItem
    {
        public int Id { get; set; }
        public int VendaId { get; set; }
        public Venda Venda { get; set; }

        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }

        public decimal Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
