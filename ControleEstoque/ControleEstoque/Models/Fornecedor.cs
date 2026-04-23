using System.ComponentModel.DataAnnotations;

public class Fornecedor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    public string NomeFantasia { get; set; }

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    public string Cnpj { get; set; }

    [Required(ErrorMessage = "A Razão Social é obrigatória")]
    public string RazaoSocial { get; set; }

    public string Telefone { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string Rua { get; set; }

    public string Bairro { get; set; }

    public string Numero { get; set; }

    public string Cidade { get; set; }
}