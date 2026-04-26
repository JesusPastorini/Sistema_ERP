using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class ContasPagar
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dê uma descrição para o gasto")]
        public string Descricao { get; set; } = string.Empty;

        // O '?' permite que o campo seja vazio (opcional)
        public string? NumeroNota { get; set; }

        // O '?' permite lançar sem selecionar um fornecedor cadastrado
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        public string? Categoria { get; set; }

        public string? UrlDocumento { get; set; }

    }
}
