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
        public async Task<IActionResult> Index(
     string fornecedor,
     DateTime? dataInicio,
     DateTime? dataFim,
     string status)
        {
            var hoje = DateTime.Today;

            var query = _context.ContasPagar
                .Include(c => c.Fornecedor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(fornecedor))
            {
                query = query.Where(c =>
                    (c.Fornecedor != null &&
                     c.Fornecedor.NomeFantasia.Contains(fornecedor))
                    ||
                    (c.NomeFornecedor != null &&
                     c.NomeFornecedor.Contains(fornecedor))
                );
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(c =>
                    c.DataVencimento.Date >= dataInicio.Value.Date);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(c =>
                    c.DataVencimento.Date <= dataFim.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "pendente")
                {
                    query = query.Where(c =>
                        c.DataPagamento == null);
                }

                if (status == "pago")
                {
                    query = query.Where(c =>
                        c.DataPagamento != null);
                }

                if (status == "vencido")
                {
                    query = query.Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento < hoje);
                }

                if (status == "vencendo")
                {
                    query = query.Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento >= hoje &&
                        c.DataVencimento <= hoje.AddDays(30));
                }
            }

            query = query
                .OrderBy(c => c.DataPagamento != null ? 3 :
                             c.DataVencimento < hoje ? 0 :
                             c.DataVencimento <= hoje.AddDays(30) ? 1 : 2)
                .ThenBy(c => c.DataVencimento);

            var contas = await query
                .Take(10)
                .ToListAsync();

            ViewBag.Fornecedor = fornecedor;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(contas);
        }

        // ================= CREATE (GET) =================
        public IActionResult Create()
        {
            var fornecedores = _context.Fornecedores
                .OrderBy(f => f.NomeFantasia)
                .ToList();

            ViewBag.FornecedorId = new SelectList(
                fornecedores,
                "Id",
                "NomeFantasia"
            );

            ViewBag.Fornecedores = fornecedores;

            return View();
        }

        // ================= CREATE (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContasPagar contasPagar)
        {
            contasPagar.DataVencimento =
                DateTime.SpecifyKind(contasPagar.DataVencimento, DateTimeKind.Utc);

            if (contasPagar.DataPagamento.HasValue)
            {
                contasPagar.DataPagamento =
                    DateTime.SpecifyKind(
                        contasPagar.DataPagamento.Value,
                        DateTimeKind.Utc);
            }

            ModelState.Remove("Fornecedor");

            if (string.IsNullOrWhiteSpace(contasPagar.NomeFornecedor)
                && contasPagar.FornecedorId == null)
            {
                ModelState.AddModelError(
                    "NomeFornecedor",
                    "Informe um fornecedor.");
            }

            if (!ModelState.IsValid)
            {
                var fornecedores = _context.Fornecedores
                    .OrderBy(f => f.NomeFantasia)
                    .ToList();

                ViewBag.FornecedorId = new SelectList(
                    fornecedores,
                    "Id",
                    "NomeFantasia",
                    contasPagar.FornecedorId
                );

                ViewBag.Fornecedores = fornecedores;

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

            var fornecedores = _context.Fornecedores
                .OrderBy(f => f.NomeFantasia)
                .ToList();

            ViewBag.FornecedorId = new SelectList(
                fornecedores,
                "Id",
                "NomeFantasia",
                conta.FornecedorId
            );

            ViewBag.Fornecedores = fornecedores;

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
                var fornecedores = _context.Fornecedores
    .OrderBy(f => f.NomeFantasia)
    .ToList();

                ViewBag.FornecedorId = new SelectList(
                    fornecedores,
                    "Id",
                    "NomeFantasia",
                    conta.FornecedorId
                );

                ViewBag.Fornecedores = fornecedores;

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