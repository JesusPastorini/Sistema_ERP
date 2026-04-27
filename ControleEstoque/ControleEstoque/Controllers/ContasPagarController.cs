using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers
{
    public class ContasPagarController : Controller
    {
        private readonly AppDbContext _context;

        public ContasPagarController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contas = await _context.ContasPagar
                .Include(c => c.Fornecedor)
                .OrderByDescending(c => c.DataVencimento)
                .ToListAsync();

            return View(contas);
        }

        // GET: ContasPagar/Create
        public IActionResult Create()
        {
            // Buscamos os fornecedores e usamos "NomeFantasia" como o texto que aparece no site
            var fornecedores = _context.Fornecedores.OrderBy(f => f.NomeFantasia).ToList();

            ViewBag.FornecedorId = new SelectList(fornecedores, "Id", "NomeFantasia");

            return View();
        }

        // POST: ContasPagar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContasPagar contasPagar)
        {
            // 1. Forçar a Data de Vencimento para UTC
            contasPagar.DataVencimento = DateTime.SpecifyKind(contasPagar.DataVencimento, DateTimeKind.Utc);

            // 2. Se houver Data de Pagamento, converter também
            if (contasPagar.DataPagamento.HasValue)
            {
                contasPagar.DataPagamento = DateTime.SpecifyKind(contasPagar.DataPagamento.Value, DateTimeKind.Utc);
            }

            // Remover validações de objetos de navegação para evitar erros de validação
            ModelState.Remove("Fornecedor");

            if (ModelState.IsValid)
            {
                _context.Add(contasPagar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FornecedorId = new SelectList(_context.Fornecedores, "Id", "NomeFantasia", contasPagar.FornecedorId);
            return View(contasPagar);
        }

        [HttpPost]
        public async Task<IActionResult> Baixar(int id)
        {
            var conta = await _context.ContasPagar.FindAsync(id);
            if (conta != null)
            {
                conta.DataPagamento = DateTime.UtcNow;
                _context.Update(conta);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
