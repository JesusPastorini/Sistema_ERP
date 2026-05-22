namespace ControleEstoque.Models.ViewModels
{
    public class DashboardViewModel
    {
        // ESTOQUE
        public int TotalProdutos { get; set; }
        public int ProdutosBaixoEstoque { get; set; }

        // MOVIMENTAÇÃO
        public int EntradasHoje { get; set; }
        public int SaidasHoje { get; set; }

        // FINANCEIRO
        public decimal ContasReceberHoje { get; set; }
        public decimal ContasPagarHoje { get; set; }
        public decimal FluxoMes { get; set; }

        // COMERCIAL
        public int VendasMes { get; set; }
        public int OrcamentosPendentes { get; set; }

        // ALERTAS
        public int ContasVencidas { get; set; }
        public int ProdutosCriticos { get; set; }
    }
}