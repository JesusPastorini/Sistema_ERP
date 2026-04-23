using ControleEstoque.Data;
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

    // GET
    public IActionResult Create()
    {
        ViewBag.Produtos = new SelectList(_context.Produtos, "Id", "Categoria");
        return View();
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> Create(MovimentacaoEstoque movimentacao)
    {
        if (ModelState.IsValid)
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
                    ModelState.AddModelError("", "Estoque insuficiente.");
                    ViewBag.Produtos = new SelectList(_context.Produtos, "Id", "Categoria");
                    return View(movimentacao);
                }

                produto.QuantidadeEstoque -= movimentacao.Quantidade;
            }

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Produto");
        }

        ViewBag.Produtos = new SelectList(_context.Produtos, "Id", "Categoria");
        return View(movimentacao);
    }

    // GET - Saida
    public IActionResult Saida()
    {
        ViewBag.Produtos = new SelectList(_context.Produtos, "Id", "Descricao");
        return View();
    }

    // POST - Saida
    [HttpPost]
    public async Task<IActionResult> Saida(int produtoId, decimal quantidade)
    {
        var produto = await _context.Produtos.FindAsync(produtoId);

        if (produto == null)
            return NotFound();

        if (produto.QuantidadeEstoque < quantidade)
        {
            ModelState.AddModelError("", "Estoque insuficiente.");
            ViewBag.Produtos = new SelectList(_context.Produtos, "Id", "Descricao");
            return View();
        }

        produto.QuantidadeEstoque -= quantidade;

        var movimentacao = new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            Quantidade = quantidade,
            TipoMovimentacao = "Saida",
            DataMovimentacao = DateTime.Now
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Produto");
    }
}