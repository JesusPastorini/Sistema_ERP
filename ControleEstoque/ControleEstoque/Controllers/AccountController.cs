using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[AllowAnonymous] // Permite acesso sem estar autenticado (necessário para Login)
public class AccountController : Controller
{
    private readonly AppDbContext _context;

    // Injeção do DbContext
    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================
    // GET: /Account/Login
    // Exibe a tela de login
    // ==========================
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // ==========================
    // POST: /Account/Login
    // Processa o login do usuário
    // ==========================
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Verifica se os dados enviados são válidos
        if (!ModelState.IsValid)
            return View(model);

        // Busca o usuário pelo email
        // Include carrega a Permissao (Role) junto
        var usuario = await _context.Usuarios
            .Include(u => u.Permissao)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        // Se não encontrou ou a senha não confere
        if (usuario == null ||
            !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha))
        {
            ModelState.AddModelError("", "Email ou senha inválidos");
            return View(model);
        }

        // ==========================
        // CRIAÇÃO DAS CLAIMS (dados que vão no cookie)
        // ==========================
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.Nome),

            new Claim(ClaimTypes.Email, usuario.Email),

            new Claim("UsuarioId", usuario.Id.ToString()),

            new Claim(ClaimTypes.Role, usuario.Permissao.NomePerfil),

            new Claim("PodeGerenciarUsuarios",
        usuario.Permissao.PodeGerenciarUsuarios.ToString())
        };

        // Cria identidade baseada no esquema de autenticação por Cookie
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        // Realiza login e cria o cookie de autenticação
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        // Redireciona para Home após login
        return RedirectToAction("Index", "Home");
    }

    // ==========================
    // GET: /Account/Logout
    // Remove o cookie e encerra sessão
    // ==========================
    public async Task<IActionResult> Logout()
    {
        // Remove o cookie de autenticação
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        // Redireciona para Login
        return RedirectToAction("Login");
    }
}
