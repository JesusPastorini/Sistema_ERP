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

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}