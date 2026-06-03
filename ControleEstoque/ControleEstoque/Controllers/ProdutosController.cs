using ControleEstoque.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = "PodeGerenciarProdutos")]
public class ProdutosController : Controller
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    // ================= INDEX =================
    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produtos
            .Include(p => p.Fornecedor)
            .OrderByDescending(p => p.DataCriacao)
            .Take(10)
            .ToListAsync();

        return View(produtos);
    }

    // ================= LOAD MORE =================
    public async Task<IActionResult> CarregarMais(int skip = 0)
    {
        var produtos = await _context.Produtos
            .Include(p => p.Fornecedor)
            .OrderByDescending(p => p.Id)
            .Skip(skip)
            .Take(20)
            .Select(p => new
            {
                id = p.Id,
                nome = p.TipoMadeira,
                dimensoes = p.Dimensoes,
                categoria = p.Categoria,
                fornecedor = p.Fornecedor != null ? p.Fornecedor.NomeFantasia : null,
                estoque = p.QuantidadeEstoque
            })
            .ToListAsync();

        return Json(produtos);
    }

    // ================= API MODAIS =================

    [HttpGet]
    public async Task<IActionResult> ObterDetalhes(int id)
    {
        var produto = await _context.Produtos
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
            return NotFound();

        return Json(new
        {
            categoria = produto.Categoria,
            tipoMadeira = produto.TipoMadeira,
            descricao = produto.Descricao,
            dimensoes = produto.Dimensoes,
            unidadeMedida = produto.UnidadeMedida,
            quantidadeEstoque = produto.QuantidadeEstoque,
            fornecedor = produto.Fornecedor?.NomeFantasia ?? "N/A"
        });
    }

    [HttpGet]
    public async Task<IActionResult> ObterProduto(int id)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (produto == null)
            return NotFound();

        return Json(new
        {
            id = produto.Id,
            tipoMadeira = produto.TipoMadeira,
            dimensoes = produto.Dimensoes
        });
    }

    // ================= CREATE =================
    public IActionResult Create()
    {
        ViewBag.Fornecedores = new SelectList(_context.Fornecedores,
                                              "Id",
                                              "NomeFantasia");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Produto produto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            foreach (var erro in erros)
            {
                Console.WriteLine(erro);
            }

            ViewBag.Fornecedores = new SelectList(_context.Fornecedores,
                                                  "Id",
                                                  "NomeFantasia");
            return View(produto);
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Fornecedores = new SelectList(_context.Fornecedores,
                                                  "Id",
                                                  "NomeFantasia");
            return View(produto);
        }

        _context.Add(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ================= DETAILS =================
    public async Task<IActionResult> Details(int id)
    {
        var produto = await _context.Produtos
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
            return NotFound();

        return View(produto);
    }

    // ================= EDIT =================
    public async Task<IActionResult> Edit(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return NotFound();

        ViewBag.Fornecedores = new SelectList(_context.Fornecedores,
                                              "Id",
                                              "NomeFantasia",
                                              produto.FornecedorId);

        return View(produto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Produto produto)
    {
        if (id != produto.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Fornecedores = new SelectList(_context.Fornecedores,
                                                  "Id",
                                                  "NomeFantasia",
                                                  produto.FornecedorId);
            return View(produto);
        }

        _context.Update(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ================= DELETE =================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto != null)
        {
            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}