using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class Orcamento
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        // 🔥 ADICIONE ISTO
        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public DateTime DataOrcamento { get; set; }

        public decimal ValorTotal { get; set; }

        public string? FormaPagamento { get; set; }

        public int? CondicaoPagamentoId { get; set; }

        public CondicaoPagamento? CondicaoPagamento { get; set; }

        public int? ValidadeDias { get; set; }

        public string? Observacoes { get; set; }

        public string Status { get; set; } = "Aberto";

        public ICollection<OrcamentoItem>? Itens { get; set; }
    }
}