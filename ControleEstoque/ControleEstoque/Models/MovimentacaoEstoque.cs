using System.ComponentModel.DataAnnotations;


namespace ControleEstoque.Models
{
    public class MovimentacaoEstoque
{
    public int Id { get; set; }

    [Required]
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; }

    [Required]
    public decimal Quantidade { get; set; }

    public string TipoMovimentacao { get; set; }
    // "Entrada" ou "Saida"

    public DateTime DataMovimentacao { get; set; } = DateTime.Now;

    public string? Descricao { get; set; }

    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }
}
}