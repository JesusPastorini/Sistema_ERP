using System.ComponentModel.DataAnnotations;

public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O tipo da madeira é obrigatório")]
    [StringLength(50)]
    public string TipoMadeira { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória")]
    [StringLength(50)]
    public string Categoria { get; set; }

    [StringLength(255)]
    public string Descricao { get; set; }

    [StringLength(50)]
    public string Dimensoes { get; set; }

    [StringLength(30)]
    public string UnidadeMedida { get; set; }

    public decimal QuantidadeEstoque { get; set; }

    // Fornecedor principal
    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}