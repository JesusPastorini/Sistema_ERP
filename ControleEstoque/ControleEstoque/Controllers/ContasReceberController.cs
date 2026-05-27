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

        public async Task<IActionResult> Index(
    int? clienteId,
    DateTime? dataInicio,
    DateTime? dataFim,
    string status)
        {
            var hoje = DateTime.Today;
            var limite = hoje.AddDays(30);

            var query = _context.ContasReceber
                .Include(c => c.Venda)
                    .ThenInclude(v => v.Cliente)
                .Include(c => c.Cliente)
                .AsQueryable();

            // CLIENTE
            if (clienteId.HasValue)
            {
                query = query.Where(c =>
                    c.ClienteId == clienteId.Value ||

                    (c.Venda != null &&
                     c.Venda.ClienteId == clienteId.Value));
            }

            // DATA
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

            // STATUS
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
                {
                    query = query.Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento >= hoje &&
                        c.DataVencimento <= limite);
                }
            }

            // 🔥 SE FILTRAR DATA → ORDENA POR DATA
            if (dataInicio.HasValue || dataFim.HasValue)
            {
                query = query.OrderBy(c => c.DataVencimento);
            }
            else
            {
                // 🔥 SEM FILTRO DE DATA → ORDENAÇÃO INTELIGENTE
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
                .Take(10)
                .ToListAsync();

            // MANTER FILTROS
            ViewBag.ClienteId = clienteId;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            // TEXTO CLIENTE SELECT2
            if (clienteId.HasValue)
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == clienteId.Value);

                ViewBag.ClienteTexto =
                    cliente != null
                        ? cliente.Nome
                        : "";
            }

            return View(contas);
        }

        // Scrool infinito
        public async Task<IActionResult> CarregarMais(
    int skip = 0,
    int? clienteId = null,
    DateTime? dataInicio = null,
    DateTime? dataFim = null,
    string status = "")
        {
            var hoje = DateTime.Today;
            var limite = hoje.AddDays(30);

            var query = _context.ContasReceber
                .Include(c => c.Venda)
                    .ThenInclude(v => v.Cliente)
                .Include(c => c.Cliente)
                .AsQueryable();

            // CLIENTE
            if (clienteId.HasValue)
            {
                query = query.Where(c =>

                    c.ClienteId == clienteId.Value ||

                    (c.Venda != null &&
                     c.Venda.ClienteId == clienteId.Value)
                );
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
                query = query.Where(c =>
                    c.DataVencimento.Date <= dataFim.Value.Date);
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

            // 🔥 SE FILTRAR DATA → ORDENA POR DATA
            if (dataInicio.HasValue || dataFim.HasValue)
            {
                query = query.OrderBy(c => c.DataVencimento);
            }
            else
            {
                // 🔥 SEM FILTRO DE DATA → ORDENAÇÃO INTELIGENTE
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

                    cliente = c.Cliente != null
                        ? c.Cliente.Nome
                        : c.Venda.Cliente.Nome,

                    vencimento = c.DataVencimento.ToString("dd/MM/yyyy"),

                    valor = c.Valor,

                    pago = c.DataPagamento != null,

                    origem = c.VendaId != null
                        ? "Venda #" + c.VendaId
                        : (c.Descricao ?? "Manual"),

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