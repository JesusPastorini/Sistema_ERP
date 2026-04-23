using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Models
{
   // [Table("usuarios")]
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }

       // [Column("permissao_id")]
        public int PermissaoId { get; set; }

        public Permissao Permissao { get; set; }
    }
}

