namespace ControleEstoque.Models
{
    public class CondicaoPagamento
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Tipo { get; set; }

        public int Parcelas { get; set; }

        public decimal Juros { get; set; }

        public int DiasRecebimento { get; set; }

        public decimal TaxaOperadora { get; set; }

        public bool Ativo { get; set; } = true;
    }
}