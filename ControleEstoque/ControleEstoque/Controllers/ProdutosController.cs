using ControleEstoque.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> CarregarMais(int skip)
    {
        var produtos = await _context.Produtos
            .Include(p => p.Fornecedor)
            .OrderByDescending(p => p.DataCriacao)
            .Skip(skip)
            .Take(10)
            .ToListAsync();

        return PartialView("_ListaProdutos", produtos);
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
    public async Task<IActionResult> Delete(int id)
    {
        var produto = await _context.Produtos
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
            return NotFound();

        return View(produto);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
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