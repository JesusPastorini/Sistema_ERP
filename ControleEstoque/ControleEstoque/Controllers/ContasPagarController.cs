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

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var contas = await _context.ContasPagar
                .Include(c => c.Fornecedor)
                .OrderByDescending(c => c.DataVencimento)
                .ToListAsync();

            return View(contas);
        }

        // ================= CREATE (GET) =================
        public IActionResult Create()
        {
            ViewBag.FornecedorId = new SelectList(
                _context.Fornecedores.OrderBy(f => f.NomeFantasia),
                "Id",
                "NomeFantasia"
            );

            return View();
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContasPagar contasPagar)
        {
            contasPagar.DataVencimento = DateTime.SpecifyKind(contasPagar.DataVencimento, DateTimeKind.Utc);

            if (contasPagar.DataPagamento.HasValue)
            {
                contasPagar.DataPagamento = DateTime.SpecifyKind(contasPagar.DataPagamento.Value, DateTimeKind.Utc);
            }

            ModelState.Remove("Fornecedor");

            if (!ModelState.IsValid)
            {
                ViewBag.FornecedorId = new SelectList(
                    _context.Fornecedores,
                    "Id",
                    "NomeFantasia",
                    contasPagar.FornecedorId
                );

                return View(contasPagar);
            }

            _context.Add(contasPagar);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT (GET) =================
        public async Task<IActionResult> Edit(int id)
        {
            var conta = await _context.ContasPagar.FindAsync(id);

            if (conta == null)
                return NotFound();

            ViewBag.FornecedorId = new SelectList(
                _context.Fornecedores,
                "Id",
                "NomeFantasia",
                conta.FornecedorId
            );

            return View(conta);
        }

        // ================= EDIT (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContasPagar conta)
        {
            if (id != conta.Id)
                return NotFound();

            conta.DataVencimento = DateTime.SpecifyKind(conta.DataVencimento, DateTimeKind.Utc);

            if (conta.DataPagamento.HasValue)
            {
                conta.DataPagamento = DateTime.SpecifyKind(conta.DataPagamento.Value, DateTimeKind.Utc);
            }

            ModelState.Remove("Fornecedor");

            if (!ModelState.IsValid)
            {
                ViewBag.FornecedorId = new SelectList(
                    _context.Fornecedores,
                    "Id",
                    "NomeFantasia",
                    conta.FornecedorId
                );

                return View(conta);
            }

            _context.Update(conta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= BAIXAR =================
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