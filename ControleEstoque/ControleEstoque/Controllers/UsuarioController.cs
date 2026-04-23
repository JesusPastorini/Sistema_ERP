using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ControleEstoque.Controllers
{
    [Authorize(Policy = "PodeGerenciarUsuarios")] // protege o controller
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            // 🔐 HASH DA SENHA
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var podeGerenciar = HttpContext.Session.GetString("PodeGerenciarUsuarios");

            if (podeGerenciar != "True")
            {
                context.Result = RedirectToAction("AcessoNegado", "Home");
            }

            base.OnActionExecuting(context);
        }
    }
}
