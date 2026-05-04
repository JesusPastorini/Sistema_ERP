using ControleEstoque.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class FornecedoresController : Controller
{
    private readonly AppDbContext _context;

    public FornecedoresController(AppDbContext context)
    {
        _context = context;
    }

    // LISTAR
    public async Task<IActionResult> Index(string busca)
    {
        var query = _context.Fornecedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();

            query = query.Where(f =>
                f.NomeFantasia.ToLower().Contains(busca) ||
                f.RazaoSocial.ToLower().Contains(busca) ||
                f.Cnpj.Contains(busca)
            );
        }

        var fornecedores = await query
            .OrderByDescending(f => f.Id)
            .Take(10)
            .ToListAsync();

        ViewBag.Busca = busca;

        return View(fornecedores);
    }

    // TELA CREATE
    public IActionResult Create()
    {
        return View();
    }

    // SALVAR NO BANCO
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Fornecedor fornecedor)
    {
        if (ModelState.IsValid)
        {
            _context.Add(fornecedor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(fornecedor);
    }

    // -- Editar --
    // GET: Fornecedor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var fornecedor = await _context.Fornecedores.FindAsync(id);

        if (fornecedor == null)
            return NotFound();

        return View(fornecedor);
    }

    // POST: Fornecedor/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Fornecedor fornecedor)
    {
        if (id != fornecedor.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(fornecedor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Fornecedores.Any(e => e.Id == fornecedor.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(fornecedor);
    }
    // -- Excluir --
    // GET: Fornecedor/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var fornecedor = await _context.Fornecedores
            .FirstOrDefaultAsync(m => m.Id == id);

        if (fornecedor == null)
            return NotFound();

        return View(fornecedor);
    }
    // Confirme exclusão
    // POST: Fornecedor/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var fornecedor = await _context.Fornecedores.FindAsync(id);

        if (fornecedor != null)
        {
            _context.Fornecedores.Remove(fornecedor);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}