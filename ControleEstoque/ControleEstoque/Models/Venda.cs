using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class Venda
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataVenda { get; set; } = DateTime.UtcNow;

        [Required]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [Required]
        public int UsuarioId { get; set; } // Quem realizou a venda
        public Usuario Usuario { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        public string FormaPagamento { get; set; } // Ex: Dinheiro, Pix, Cartão

        // Relacionamento com os itens vendidos
        public ICollection<VendaItem> Itens { get; set; }
    }
}
