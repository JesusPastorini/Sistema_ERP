using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;
using System.Security.Claims;

namespace ControleEstoque.Controllers
{
    public class VendasController : Controller
    {
        private readonly AppDbContext _context;

        public VendasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound();

            return View(venda);
        }

        public async Task<IActionResult> Index()
        {
            var vendas = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
            return View(vendas);
        }

        public IActionResult Create()
        {
            ViewBag.ClienteId = new SelectList(_context.Set<Cliente>().OrderBy(c => c.Nome), "Id", "Nome");
            return View();
        }

        // NOVO MÉTODO: Busca com Paginação (Scroll Infinito) e Filtro em 3 campos
        [HttpGet]
        public async Task<IActionResult> BuscarProdutosPaginado(string termo, int pagina = 1)
        {
            int itensPorPagina = 10;
            var consulta = _context.Produtos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                var busca = termo.ToLower();
                // Busca flexível: Categoria OR Tipo OR Dimensões
                consulta = consulta.Where(p =>
                    p.Categoria.ToLower().Contains(busca) ||
                    p.TipoMadeira.ToLower().Contains(busca) ||
                    p.Dimensoes.ToLower().Contains(busca));
            }

            var totalItens = await consulta.CountAsync();

            var produtos = await consulta
                .OrderBy(p => p.TipoMadeira)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(p => new {
                    id = p.Id,
                    text = $"{p.Categoria} - {p.TipoMadeira} ({p.Dimensoes})",
                    estoque = p.QuantidadeEstoque
                })
                .ToListAsync();

            // O Select2 espera o campo 'more' para saber se continua o scroll
            return Json(new
            {
                items = produtos,
                pagination = new { more = (pagina * itensPorPagina) < totalItens }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    Venda venda,
    int[] ProdutoId,
    decimal[] Quantidade,
    decimal[] PrecoUnitario,
    int QtdParcelas,
    decimal Juros)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            venda.UsuarioId = string.IsNullOrEmpty(userIdClaim)
                ? (await _context.Usuarios.FirstOrDefaultAsync())?.Id ?? 1
                : int.Parse(userIdClaim);

            venda.DataVenda = DateTime.UtcNow;

            ModelState.Remove("Usuario");
            ModelState.Remove("Cliente");
            ModelState.Remove("Itens");

            // 🔴 VALIDAÇÃO ITENS
            if (ProdutoId == null || ProdutoId.Length == 0)
            {
                ModelState.AddModelError("", "⚠️ Adicione ao menos um item na venda.");
            }
            else
            {
                for (int i = 0; i < ProdutoId.Length; i++)
                {
                    var produto = await _context.Produtos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == ProdutoId[i]);

                    if (produto == null || produto.QuantidadeEstoque < Quantidade[i])
                    {
                        ModelState.AddModelError("", $"❌ Estoque insuficiente para {produto?.TipoMadeira}.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ClienteId = new SelectList(
                    _context.Set<Cliente>().OrderBy(c => c.Nome),
                    "Id",
                    "Nome",
                    venda.ClienteId
                );

                return View(venda);
            }

            // 🔥 SALVA VENDA
            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            decimal totalCalculado = 0;

            // 🔥 SALVA ITENS + BAIXA ESTOQUE
            for (int i = 0; i < ProdutoId.Length; i++)
            {
                var pId = ProdutoId[i];
                var qtd = Quantidade[i];
                var preco = PrecoUnitario[i];

                _context.VendaItens.Add(new VendaItem
                {
                    VendaId = venda.Id,
                    ProdutoId = pId,
                    Quantidade = qtd,
                    PrecoUnitario = preco
                });

                var prodEstoque = await _context.Produtos.FindAsync(pId);
                if (prodEstoque != null)
                    prodEstoque.QuantidadeEstoque -= qtd;

                totalCalculado += (qtd * preco);
            }

            venda.ValorTotal = totalCalculado;

            // =========================
            // 🔥 PARCELAMENTO INTELIGENTE
            // =========================

            int parcelas = QtdParcelas <= 0 ? 1 : QtdParcelas;

            // 🔥 REGRA PROFISSIONAL (máximo 12)
            if (parcelas > 12)
                parcelas = 12;

            decimal jurosValor = Juros > 0 ? totalCalculado * (Juros / 100) : 0;
            decimal totalComJuros = totalCalculado + jurosValor;
            decimal valorParcela = totalComJuros / parcelas;

            for (int i = 1; i <= parcelas; i++)
            {
                _context.ContasReceber.Add(new ContasReceber
                {
                    VendaId = venda.Id,
                    Valor = valorParcela,
                    DataVencimento = DateTime.UtcNow.AddDays(30 * i),
                    Observacao = parcelas > 1
                        ? $"Parcela {i}/{parcelas} - Venda #{venda.Id}"
                        : $"Venda #{venda.Id}"
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
