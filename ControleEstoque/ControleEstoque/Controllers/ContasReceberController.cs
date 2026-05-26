using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleEstoque.Controllers
{
    public class ContasReceberController : Controller
    {
        private readonly AppDbContext _context;

        public ContasReceberController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string cliente, DateTime? dataInicio, DateTime? dataFim, string status)
        {
            var hoje = DateTime.Today;
            var limite = hoje.AddDays(30);

            var query = _context.ContasReceber
                .Include(c => c.Venda)
                    .ThenInclude(v => v.Cliente)
                .Include(c => c.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cliente))
            {
                query = query.Where(c =>
                    (c.Cliente != null && c.Cliente.Nome.Contains(cliente)) ||
                    (c.Venda != null && c.Venda.Cliente.Nome.Contains(cliente))
                );
            }

            // 🔥 NOVO FILTRO POR PERÍODO
            if (dataInicio.HasValue)
            {
                query = query.Where(c => c.DataVencimento >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(c => c.DataVencimento <= dataFim.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "pendente")
                    query = query.Where(c => c.DataPagamento == null);

                if (status == "pago")
                    query = query.Where(c => c.DataPagamento != null);
                if (status == "vencido")
                {
                    query = query.Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento < hoje);
                }

                if (status == "vencendo")
                    query = query.Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento >= hoje &&
                        c.DataVencimento <= limite);
            }

            // 🔥 NOVA ORDENAÇÃO INTELIGENTE
            var contas = await query
                .OrderBy(c =>
                    c.DataPagamento != null ? 4 :
                    c.DataVencimento < hoje ? 1 :
                    c.DataVencimento <= limite ? 2 :
                    3
                )
                .ThenBy(c => c.DataVencimento)
                .Take(10)
                .ToListAsync();

            ViewBag.Cliente = cliente;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            return View(contas);
        }

        // Scrool infinito
        public async Task<IActionResult> CarregarMais(int skip = 0)
        {
            var hoje = DateTime.Now;
            var limite = hoje.AddDays(30);

            var contas = await _context.ContasReceber
                .Include(c => c.Venda)
                .ThenInclude(v => v.Cliente)
                .Include(c => c.Cliente)
                .OrderBy(c =>
                    c.DataPagamento != null ? 4 :
                    c.DataVencimento < hoje ? 1 :
                    c.DataVencimento <= limite ? 2 :
                    3
                )
                .ThenBy(c => c.DataVencimento)
                .Skip(skip)
                .Take(20)
                .Select(c => new
                {
                    id = c.Id,

                    cliente = c.Cliente != null
                        ? c.Cliente.Nome
                        : c.Venda.Cliente.Nome,

                    vencimento = c.DataVencimento.ToString("dd/MM/yyyy"),

                    valor = c.Valor,

                    pago = c.DataPagamento != null,

                    origem = c.VendaId != null
                        ? "Venda #" + c.VendaId
                        : (c.Descricao ?? "Manual"),

                    // 🔥 NOVOS CAMPOS
                    vencido = c.DataPagamento == null && c.DataVencimento < hoje,

                    vencendo = c.DataPagamento == null &&
                               c.DataVencimento >= hoje &&
                               c.DataVencimento <= limite
                })
                .ToListAsync();

            return Json(contas);
        }

        // Cria manual
        public IActionResult Create()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nome");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContasReceber conta)
        {
            if (!ModelState.IsValid)
            {
                foreach (var erro in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(erro.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(conta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nome");
            return View(conta);
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var conta = await _context.ContasReceber
                .Include(c => c.Cliente)
                .Include(c => c.Venda)
                    .ThenInclude(v => v.Cliente)
                .Include(c => c.Venda)
                    .ThenInclude(v => v.Itens)
                        .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null)
                return NotFound();

            return View(conta);
        }

        // Método para dar baixa (pagar)
        [HttpPost]
        public async Task<IActionResult> Baixar(int id)
        {
            var conta = await _context.ContasReceber.FindAsync(id);
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