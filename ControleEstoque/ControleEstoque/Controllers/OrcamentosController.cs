using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControleEstoque.Controllers
{
    [Authorize]
    public class OrcamentosController : Controller
    {
        private readonly AppDbContext _context;

        public OrcamentosController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var orcamentos = await _context.Orcamentos
                .Include(o => o.Cliente)
                .Include(o => o.Usuario)
                .OrderByDescending(o => o.DataOrcamento)
                .ToListAsync();

            return View(orcamentos);
        }

        // =========================
        // Details
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var orcamento = await _context.Orcamentos
                .Include(o => o.Cliente)
                .Include(o => o.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orcamento == null)
                return NotFound();

            return View(orcamento);
        }

        // =========================
        // CREATE GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ClienteId = new SelectList(
                _context.Clientes.OrderBy(c => c.Nome),
                "Id",
                "Nome"
            );

            return View();
        }

        // =========================
        // CREATE POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Orcamento orcamento,
            int[] ProdutoId,
            decimal[] Quantidade,
            decimal[] PrecoUnitario)
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("Usuario");
            ModelState.Remove("Itens");

            if (ProdutoId == null || ProdutoId.Length == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Adicione ao menos um item.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ClienteId = new SelectList(
                    _context.Clientes.OrderBy(c => c.Nome),
                    "Id",
                    "Nome",
                    orcamento.ClienteId
                );

                return View(orcamento);
            }

            // usuário logado
            var usuarioId = User.FindFirst("UsuarioId")?.Value;

            orcamento.UsuarioId = int.Parse(usuarioId!);

            // total
            decimal total = 0;

            for (int i = 0; i < ProdutoId.Length; i++)
            {
                total += Quantidade[i] * PrecoUnitario[i];
            }

            orcamento.ValorTotal = total;

            _context.Orcamentos.Add(orcamento);

            await _context.SaveChangesAsync();

            // itens
            for (int i = 0; i < ProdutoId.Length; i++)
            {
                var subtotal =
                    Quantidade[i] * PrecoUnitario[i];

                _context.OrcamentoItens.Add(
                    new OrcamentoItem
                    {
                        OrcamentoId = orcamento.Id,
                        ProdutoId = ProdutoId[i],
                        Quantidade = Quantidade[i],
                        PrecoUnitario = PrecoUnitario[i],
                        Subtotal = subtotal
                    });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}