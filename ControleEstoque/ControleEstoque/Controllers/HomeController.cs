using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers
{
    // 🔐 Só usuário autenticado pode acessar
    [Authorize]
    public class HomeController : Controller
    {
        // Página principal
        public IActionResult Index()
        {
            return View();
        }
    }
}