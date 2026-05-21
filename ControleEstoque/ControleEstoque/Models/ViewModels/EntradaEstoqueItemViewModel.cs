namespace ControleEstoque.Models.ViewModels
{
    public class EntradaEstoqueItemViewModel
    {
        // NOME VINDO DO XML
        public string NomeXml { get; set; } = "";

        // PRODUTO DO SISTEMA
        public int? ProdutoId { get; set; }

        // DADOS
        public decimal Quantidade { get; set; }

        public decimal ValorCusto { get; set; }
    }
}