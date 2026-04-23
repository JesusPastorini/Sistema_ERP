using System.ComponentModel.DataAnnotations;

public class MovimentacaoEstoque
{
    public int Id { get; set; }

    [Required]
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; }

    [Required]
    public decimal Quantidade { get; set; }

    [Required]
    public string TipoMovimentacao { get; set; }
    // "Entrada" ou "Saida"

    public DateTime DataMovimentacao { get; set; } = DateTime.Now;
}