using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class MovimentacaoEstoque
    {
        public int Id { get; set; }

        // PRODUTO ORIGEM
        [Required]
        public int ProdutoId { get; set; }

        public Produto Produto { get; set; }

        // PRODUTO DESTINO
        public int? ProdutoDestinoId { get; set; }

        public Produto? ProdutoDestino { get; set; }

        // DADOS
        [Required]
        public decimal Quantidade { get; set; }

        public decimal? ValorUnitario { get; set; }

        public string TipoMovimentacao { get; set; }
        // Entrada
        // Saida
        // Transferencia
        // Ajuste

        public DateTime DataMovimentacao { get; set; }
            = DateTime.Now;

        public string? Descricao { get; set; }

        // VENDA
        public int? VendaId { get; set; }

        public Venda? Venda { get; set; }
    }
}