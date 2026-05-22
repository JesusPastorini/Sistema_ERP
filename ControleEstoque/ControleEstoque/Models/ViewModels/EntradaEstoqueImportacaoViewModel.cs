namespace ControleEstoque.Models.ViewModels
{
	public class EntradaEstoqueImportacaoViewModel
	{
		public string NumeroNotaFiscal { get; set; } = "";

		public string NomeFornecedor { get; set; } = "";

		public int? FornecedorId { get; set; }

		public decimal ValorTotal { get; set; }

		public List<EntradaEstoqueItemViewModel> Itens { get; set; }
			= new();
	}
}