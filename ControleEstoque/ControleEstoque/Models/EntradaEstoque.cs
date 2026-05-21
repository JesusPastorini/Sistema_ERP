using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class EntradaEstoque
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataEntrada { get; set; }

        public string? NumeroNotaFiscal { get; set; }

        public string TipoEntrada { get; set; } = "Manual";
        // Manual | NotaFiscal

        public string? Observacao { get; set; }

        // FORNECEDOR
        public int? FornecedorId { get; set; }

        public Fornecedor? Fornecedor { get; set; }

        public decimal ValorTotal { get; set; }

        // ITENS
        public ICollection<EntradaEstoqueItem> Itens { get; set; }
            = new List<EntradaEstoqueItem>();
    }
}