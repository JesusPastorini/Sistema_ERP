using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class ContasReceber
    {
        public int Id { get; set; }

        public int? VendaId { get; set; }
        public Venda? Venda { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        public string? Observacao { get; set; }

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string? Descricao { get; set; }
    }
}
