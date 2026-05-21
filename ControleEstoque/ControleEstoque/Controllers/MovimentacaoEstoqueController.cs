using ControleEstoque.Data;
using ControleEstoque.Models;
using ControleEstoque.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class MovimentacaoEstoqueController : Controller
{
    private readonly AppDbContext _context;

    public MovimentacaoEstoqueController(AppDbContext context)
    {
        _context = context;
    }

    // 🔹 TELA PRINCIPAL
    public async Task<IActionResult> Index(string busca, DateTime? dataInicio, DateTime? dataFim)
    {
        ViewBag.Produtos = new SelectList(
            _context.Produtos
                .Select(p => new
                {
                    p.Id,
                    NomeCompleto = p.TipoMadeira + " - " + p.Categoria + " (" + p.Dimensoes + ")"
                }),
            "Id",
            "NomeCompleto"
        );

        var query = _context.MovimentacoesEstoque
            .Include(m => m.Produto)
            .Include(m => m.Venda)
            .AsQueryable();

        // 🔎 BUSCA
        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();

            query = query.Where(m =>
                m.Produto.TipoMadeira.ToLower().Contains(busca) ||
                m.Produto.Categoria.ToLower().Contains(busca)
            );
        }

        // 📅 FILTRO DATA
        if (dataInicio.HasValue)
            query = query.Where(m => m.DataMovimentacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(m => m.DataMovimentacao <= dataFim.Value);

        var historico = await query
            .OrderByDescending(m => m.DataMovimentacao)
            .Take(50)
            .ToListAsync();

        return View(historico);
    }

    // Scrool infinito
    public async Task<IActionResult> CarregarMais(int skip = 0)
    {
        var movs = await _context.MovimentacoesEstoque
            .OrderByDescending(m => m.DataMovimentacao)
            .Skip(skip)
            .Take(20)
            .Select(m => new
            {
                data = m.DataMovimentacao.ToString("dd/MM HH:mm"),
                produto = m.Produto.TipoMadeira,
                categoria = m.Produto.Categoria,
                dimensoes = m.Produto.Dimensoes,
                tipo = m.TipoMovimentacao,
                quantidade = m.Quantidade,
                vendaId = m.VendaId,
                descricao = m.Descricao
            })
            .ToListAsync();

        return Json(movs);
    }

    // ==========================
    // CREATE GET
    // ==========================
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Produtos = new SelectList(
            _context.Produtos
                .Select(p => new
                {
                    p.Id,
                    NomeCompleto =
                        p.TipoMadeira + " - " +
                        p.Categoria + " (" +
                        p.Dimensoes + ")"
                }),
            "Id",
            "NomeCompleto"
        );

        return View();
    }

    // ==========================
    // CREATE POST
    // ==========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovimentacaoEstoque movimentacao)
    {
        ViewBag.Produtos = new SelectList(
            _context.Produtos
                .Select(p => new
                {
                    p.Id,
                    NomeCompleto =
                        p.TipoMadeira + " - " +
                        p.Categoria + " (" +
                        p.Dimensoes + ")"
                }),
            "Id",
            "NomeCompleto"
        );

        var produtoOrigem =
            await _context.Produtos
            .FindAsync(movimentacao.ProdutoId);

        if (produtoOrigem == null)
        {
            ModelState.AddModelError("", "Produto inválido.");
            return View(movimentacao);
        }

        // ENTRADA
        if (movimentacao.TipoMovimentacao == "Entrada")
        {
            decimal estoqueAnterior =
    produtoOrigem.QuantidadeEstoque;

            produtoOrigem.QuantidadeEstoque =
                movimentacao.Quantidade;

            movimentacao.Descricao =
                $"Ajuste manual: {estoqueAnterior} -> {movimentacao.Quantidade}";
        }

        // SAÍDA
        else if (movimentacao.TipoMovimentacao == "Saida")
        {
            if (produtoOrigem.QuantidadeEstoque < movimentacao.Quantidade)
            {
                ModelState.AddModelError("", "Estoque insuficiente.");
                return View(movimentacao);
            }

            produtoOrigem.QuantidadeEstoque -= movimentacao.Quantidade;
        }

        // TRANSFERÊNCIA
        else if (movimentacao.TipoMovimentacao == "Transferencia")
        {
            if (!movimentacao.ProdutoDestinoId.HasValue)
            {
                ModelState.AddModelError("", "Selecione o produto destino.");
                return View(movimentacao);
            }

            var produtoDestino =
                await _context.Produtos
                .FindAsync(movimentacao.ProdutoDestinoId.Value);

            if (produtoDestino == null)
            {
                ModelState.AddModelError("", "Produto destino inválido.");
                return View(movimentacao);
            }

            if (produtoOrigem.QuantidadeEstoque < movimentacao.Quantidade)
            {
                ModelState.AddModelError("", "Estoque insuficiente.");
                return View(movimentacao);
            }

            // REMOVE DA ORIGEM
            produtoOrigem.QuantidadeEstoque -= movimentacao.Quantidade;

            // ADICIONA NO DESTINO
            produtoDestino.QuantidadeEstoque += movimentacao.Quantidade;
        }

        // AJUSTE
        else if (movimentacao.TipoMovimentacao == "Ajuste")
        {
            produtoOrigem.QuantidadeEstoque = movimentacao.Quantidade;
        }

        movimentacao.DataMovimentacao = DateTime.Now;

        _context.MovimentacoesEstoque.Add(movimentacao);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ObterEstoque(int id)
    {
        var produto = await _context.Produtos
            .FindAsync(id);

        if (produto == null)
            return NotFound();

        return Json(produto.QuantidadeEstoque);
    }

    // Buscar Produtos
    [HttpGet]
    public async Task<IActionResult> BuscarProdutosPaginado(string termo, int pagina = 1)
    {
        int itensPorPagina = 10;

        var consulta = _context.Produtos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            termo = termo.ToLower();

            consulta = consulta.Where(p =>
                p.Categoria.ToLower().Contains(termo) ||
                p.TipoMadeira.ToLower().Contains(termo) ||
                p.Dimensoes.ToLower().Contains(termo));
        }

        var totalItens = await consulta.CountAsync();

        var produtos = await consulta
            .OrderBy(p => p.TipoMadeira)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .Select(p => new
            {
                id = p.Id,

                text =
                    p.Categoria + " - " +
                    p.TipoMadeira + " - " +
                    p.Dimensoes,

                estoque = p.QuantidadeEstoque
            })
            .ToListAsync();

        return Json(new
        {
            items = produtos,
            pagination = new
            {
                more = (pagina * itensPorPagina) < totalItens
            }
        });
    }
}