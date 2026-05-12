namespace ControleEstoque.Models
{
    public class Permissao
    {
        public int Id { get; set; }

        public string NomePerfil { get; set; }

        // USUÁRIOS
        public bool PodeGerenciarUsuarios { get; set; }

        // FINANCEIRO
        public bool PodeVerFinanceiro { get; set; }

        // ESTOQUE
        public bool PodeGerenciarEstoque { get; set; }

        // PRODUTOS
        public bool PodeGerenciarProdutos { get; set; }

        // VENDAS
        public bool PodeGerenciarVendas { get; set; }

        // CLIENTES
        public bool PodeGerenciarClientes { get; set; }

        public ICollection<Usuario>? Usuarios { get; set; }
    }
}