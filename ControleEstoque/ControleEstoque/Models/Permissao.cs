using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Models
{
    public class Permissao
    {
        public int Id { get; set; }

        public string NomePerfil { get; set; }  // mapeia nome_perfil

        public bool PodeGerenciarUsuarios { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
