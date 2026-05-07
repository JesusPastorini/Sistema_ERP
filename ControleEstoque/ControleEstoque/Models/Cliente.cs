using System.ComponentModel.DataAnnotations;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O CPF/CNPJ é obrigatório")]
    public string CpfCnpj { get; set; }

    public string Telefone { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string? Cep { get; set; }

    public string? Rua { get; set; }

    public string? Numero { get; set; }

    public string? Complemento { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}