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
                    .Include(v => v.CondicaoPagamento)
                    .Include(v => v.ContasReceber)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound();

            return View(venda);
        }

        public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim, string cliente)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.CondicaoPagamento)
                .AsQueryable();

            // 📅 DATA INICIAL
            if (dataInicio.HasValue)
            {
                query = query.Where(v => v.DataVenda >= dataInicio.Value);
                ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
            }

            // 📅 DATA FINAL
            if (dataFim.HasValue)
            {
                query = query.Where(v => v.DataVenda <= dataFim.Value);
                ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
            }

            // 🔎 CLIENTE
            if (!string.IsNullOrWhiteSpace(cliente))
            {
                var busca = cliente.ToLower();

                query = query.Where(v =>
                    v.Cliente.Nome.ToLower().Contains(busca)
                );

                ViewBag.Cliente = cliente;
            }

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .Take(10)
                .ToListAsync();

            return View(vendas);
        }

        public IActionResult Create()
        {
            ViewBag.ClienteId = new SelectList(
                _context.Set<Cliente>().OrderBy(c => c.Nome),
                "Id",
                "Nome"
            );
            ViewBag.Condicoes = _context.CondicoesPagamento
             .Where(c => c.Ativo)
             .OrderBy(c => c.Nome)
             .Select(c => new
             {
                 Id = c.Id,
                 Nome = c.Nome,
                 Juros = c.Juros,
                 Parcelas = c.Parcelas,
                 TaxaOperadora = c.TaxaOperadora
             })
             .ToList();

            return View();
        }

        // Scroll de busca degrade
        [HttpGet]
        public async Task<IActionResult> CarregarMais(int skip, DateTime? dataInicio, DateTime? dataFim, string cliente)
        {
            var query = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.CondicaoPagamento)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(v => v.DataVenda >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(v => v.DataVenda <= dataFim.Value);

            if (!string.IsNullOrWhiteSpace(cliente))
                query = query.Where(v => v.Cliente.Nome.ToLower().Contains(cliente.ToLower()));

            var vendas = await query
                .OrderByDescending(v => v.DataVenda)
                .Skip(skip)
                .Take(20) // 🔥 próximos = 20
                .Select(v => new
                {
                    id = v.Id,
                    data = v.DataVenda.ToString("dd/MM/yyyy"),
                    cliente = v.Cliente.Nome,
                    usuario = v.Usuario.Nome,
                    total = v.ValorTotal,
                    pagamento = v.FormaPagamento,
                    parcelas = v.CondicaoPagamento != null ? v.CondicaoPagamento.Parcelas + "x" : "-"
                })
                .ToListAsync();

            return Json(vendas);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProdutosPaginado(string termo, int pagina = 1)
        {
            int itensPorPagina = 10;
            var consulta = _context.Produtos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                var busca = termo.ToLower();

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
                .Select(p => new
                {
                    id = p.Id,
                    text = $"{p.Categoria} - {p.TipoMadeira} ({p.Dimensoes})",
                    estoque = p.QuantidadeEstoque
                })
                .ToListAsync();

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
            decimal Juros,
            int? CondicaoPagamentoId)
        {          
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            venda.UsuarioId = string.IsNullOrEmpty(userIdClaim)
                ? (await _context.Usuarios.FirstOrDefaultAsync())?.Id ?? 1
                : int.Parse(userIdClaim);

            venda.DataVenda = DateTime.UtcNow;

            ModelState.Remove("Usuario");
            ModelState.Remove("Cliente");
            ModelState.Remove("Itens");
            ModelState.Remove("Movimentacoes");

            // =========================
            // 🔴 VALIDAÇÃO DE ITENS
            // =========================
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

            // =========================
            // 💰 CALCULA TOTAL
            // =========================
            decimal totalCalculado = 0;

            if (ProdutoId != null)
            {
                for (int i = 0; i < ProdutoId.Length; i++)
                {
                    totalCalculado += (Quantidade[i] * PrecoUnitario[i]);
                }
            }

            venda.ValorTotal = totalCalculado;

            // =========================
            // 💳 CONDIÇÃO DE PAGAMENTO
            // =========================
            var condicao = CondicaoPagamentoId.HasValue
                ? await _context.CondicoesPagamento
                    .FirstOrDefaultAsync(c => c.Id == CondicaoPagamentoId)
                : null;

            if (CondicaoPagamentoId.HasValue && condicao == null)
            {
                ModelState.AddModelError("", "Condição de pagamento inválida.");
            }

            // =========================
            // ❌ SE INVÁLIDO → VOLTA
            // =========================
            if (!ModelState.IsValid)
            {
                ViewBag.ClienteId = new SelectList(
                    _context.Set<Cliente>().OrderBy(c => c.Nome),
                    "Id",
                    "Nome",
                    venda.ClienteId
                );

                ViewBag.Condicoes = _context.CondicoesPagamento
     .Where(c => c.Ativo)
     .OrderBy(c => c.Nome)
     .Select(c => new
     {
         Id = c.Id,
         Nome = c.Nome,
         Juros = c.Juros,
         Parcelas = c.Parcelas,
         TaxaOperadora = c.TaxaOperadora
     })
     .ToList();

                return View(venda);
            }

            // =========================
            // 💾 SALVA VENDA
            // =========================
            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();
            // =========================
            // 📦 SALVA ITENS + ESTOQUE
            // =========================
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

                if (prodEstoque == null || prodEstoque.QuantidadeEstoque < qtd)
                {
                    throw new Exception("Estoque inconsistente no momento da venda.");
                }

                prodEstoque.QuantidadeEstoque -= qtd;

                var movimentacao = new MovimentacaoEstoque
                {
                    ProdutoId = pId,
                    Quantidade = qtd,
                    TipoMovimentacao = "Venda",
                    DataMovimentacao = DateTime.Now,
                    VendaId = venda.Id,
                    Descricao = null
                };

                _context.MovimentacoesEstoque.Add(movimentacao);
            }

            // =========================
            // 💸 PARCELAMENTO
            // =========================
            int parcelas = condicao?.Parcelas ?? (QtdParcelas <= 0 ? 1 : QtdParcelas);

            if (parcelas > 12)
                parcelas = 12;

            decimal jurosPercentual = condicao?.Juros ?? Juros;
            int diasRecebimento = condicao?.DiasRecebimento ?? 30;

            decimal jurosValor = jurosPercentual > 0
                ? totalCalculado * (jurosPercentual / 100)
                : 0;

            decimal totalComJuros = totalCalculado + jurosValor;

            decimal valorBase = Math.Floor((totalComJuros / parcelas) * 100) / 100;
            decimal resto = totalComJuros - (valorBase * parcelas);

            for (int i = 1; i <= parcelas; i++)
            {
                decimal valorFinal = valorBase;

                if (i == parcelas)
                    valorFinal += resto;

                _context.ContasReceber.Add(new ContasReceber
                {
                    VendaId = venda.Id,
                    Valor = valorFinal,
                    DataVencimento = DateTime.UtcNow.AddDays(diasRecebimento * i),
                    Observacao = parcelas > 1
                        ? $"Parcela {i}/{parcelas} - {condicao?.Nome ?? "Venda"} #{venda.Id}"
                        : $"Venda #{venda.Id}"
                });
            }

            venda.CondicaoPagamentoId = CondicaoPagamentoId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}