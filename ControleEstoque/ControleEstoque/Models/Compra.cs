public class Compra
{
    public int Id { get; set; }

    public DateTime DataCompra { get; set; }

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; }

    public decimal ValorTotal { get; set; }

}