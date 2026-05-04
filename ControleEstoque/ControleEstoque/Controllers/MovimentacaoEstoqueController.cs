using ControleEstoque.Data;
using ControleEstoque.Models;
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

    // 🔹 MOVIMENTAR
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Movimentar(MovimentacaoEstoque movimentacao)
    {
        var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);

        if (produto == null)
            return NotFound();

        if (movimentacao.TipoMovimentacao == "Entrada")
        {
            produto.QuantidadeEstoque += movimentacao.Quantidade;
        }
        else if (movimentacao.TipoMovimentacao == "Saida")
        {
            if (produto.QuantidadeEstoque < movimentacao.Quantidade)
            {
                ModelState.AddModelError("", "❌ Estoque insuficiente.");
                return RedirectToAction(nameof(Index));
            }

            produto.QuantidadeEstoque -= movimentacao.Quantidade;
        }

        movimentacao.DataMovimentacao = DateTime.Now;

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}