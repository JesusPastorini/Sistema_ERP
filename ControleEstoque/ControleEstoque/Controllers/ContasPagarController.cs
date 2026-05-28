using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;

namespace ControleEstoque.Controllers
{
    [Authorize(Policy = "PodeVerFinanceiro")]
    public class ContasPagarController : Controller
    {
        private readonly AppDbContext _context;

        public ContasPagarController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index(
            int? fornecedorId,
            DateTime? dataInicio,
            DateTime? dataFim,
            string status)
        {
            var hoje = DateTime.Today;

            var query = _context.ContasPagar
                .Include(c => c.Fornecedor)
                .AsQueryable();

            // FILTRO FORNECEDOR
            if (fornecedorId.HasValue)
            {
                query = query.Where(c =>
                    c.FornecedorId == fornecedorId.Value);
            }

            // DATA INICIAL
            if (dataInicio.HasValue)
            {
                query = query.Where(c =>
                    c.DataVencimento.Date >= dataInicio.Value.Date);
            }

            // DATA FINAL
            if (dataFim.HasValue)
            {
                var fimDoDia = dataFim.Value.Date.AddDays(1);

                query = query.Where(c =>
                    c.DataVencimento < fimDoDia);
            }

            // STATUS
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

            // ORDENAÇÃO
            if (dataInicio.HasValue || dataFim.HasValue)
            {
                query = query
                    .OrderBy(c => c.DataVencimento);
            }
            else
            {
                // SEM FILTRO DE DATA
                // ORDENA POR STATUS
                query = query
                    .OrderBy(c =>
                        c.DataPagamento != null ? 3 :
                        c.DataVencimento < hoje ? 0 :
                        c.DataVencimento <= hoje.AddDays(30) ? 1 : 2)
                    .ThenBy(c => c.DataVencimento);
            }

            var contas = await query
                .Take(10)
                .ToListAsync();

            // MANTER FILTROS
            ViewBag.FornecedorId = fornecedorId;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            // NOME DO FORNECEDOR PARA O SELECT2
            if (fornecedorId.HasValue)
            {
                var fornecedor = await _context.Fornecedores
                    .FirstOrDefaultAsync(f => f.Id == fornecedorId.Value);

                ViewBag.FornecedorTexto =
                    fornecedor != null
                        ? $"{fornecedor.NomeFantasia} - {fornecedor.RazaoSocial}"
                        : "";
            }

            return View(contas);
        }

        // ==========================================
        // SCROLL INFINITO
        // ==========================================
        public async Task<IActionResult> CarregarMais(
            int skip = 0,
            int? fornecedorId = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            string status = "")
        {
            var hoje = DateTime.Today;
            var limite = hoje.AddDays(30);

            var query = _context.ContasPagar
                .Include(c => c.Fornecedor)
                .AsQueryable();

            // FORNECEDOR
            if (fornecedorId.HasValue)
            {
                query = query.Where(c =>
                    c.FornecedorId == fornecedorId.Value);
            }

            // DATA INICIAL
            if (dataInicio.HasValue)
            {
                query = query.Where(c =>
                    c.DataVencimento >= dataInicio.Value);
            }

            // DATA FINAL
            if (dataFim.HasValue)
            {
                var fimDoDia = dataFim.Value.Date.AddDays(1);

                query = query.Where(c =>
                    c.DataVencimento < fimDoDia);
            }

            // STATUS
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
                        c.DataVencimento <= limite);
                }
            }

            // ORDENAÇÃO
            if (dataInicio.HasValue || dataFim.HasValue)
            {
                query = query
                    .OrderBy(c => c.DataVencimento);
            }
            else
            {
                query = query
                    .OrderBy(c =>
                        c.DataPagamento != null ? 4 :
                        c.DataVencimento < hoje ? 1 :
                        c.DataVencimento <= limite ? 2 :
                        3
                    )
                    .ThenBy(c => c.DataVencimento);
            }

            var contas = await query
                            .Skip(skip)
                .Take(20)
                .Select(c => new
                {
                    id = c.Id,

                    fornecedor =
                        c.Fornecedor != null
                            ? c.Fornecedor.NomeFantasia
                            : (c.NomeFornecedor ?? "Sem fornecedor"),

                    vencimento =
                        c.DataVencimento.ToString("dd/MM/yyyy"),

                    descricao = c.Descricao,

                    categoria = c.Categoria,

                    valor = c.Valor,

                    documento = c.UrlDocumento,

                    pago = c.DataPagamento != null,

                    vencido =
                        c.DataPagamento == null &&
                        c.DataVencimento < hoje,

                    vencendo =
                        c.DataPagamento == null &&
                        c.DataVencimento >= hoje &&
                        c.DataVencimento <= limite
                })
                .ToListAsync();

            return Json(contas);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Baixar(
            int id,
            DateTime? dataPagamento)
        {
            var conta = await _context.ContasPagar
                .FindAsync(id);

            if (conta == null)
                return NotFound();

            conta.DataPagamento =
                dataPagamento?.Date
                ?? DateTime.Today;

            _context.Update(conta);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var conta = await _context.ContasPagar
                .FindAsync(id);

            if (conta == null)
                return NotFound();

            _context.ContasPagar.Remove(conta);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}