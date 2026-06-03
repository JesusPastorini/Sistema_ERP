using ControleEstoque.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = "PodeGerenciarClientes")]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    // LISTAR
    public async Task<IActionResult> Index(string busca)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();

            query = query.Where(c =>
                c.Nome.ToLower().Contains(busca) ||
                c.CpfCnpj.Contains(busca)
            );
        }

        var clientes = await query
            .OrderByDescending(c => c.Id)
            .Take(10)
            .ToListAsync();

        ViewBag.Busca = busca;

        return View(clientes);
    }

    // Scroll infinito
    public async Task<IActionResult> CarregarMais(int skip = 0)
    {
        var clientes = await _context.Clientes
            .OrderByDescending(c => c.Id)
            .Skip(skip)
            .Take(20)
            .Select(c => new
            {
                id = c.Id,
                nome = c.Nome,
                cpfCnpj = c.CpfCnpj,
                telefone = c.Telefone,
                email = c.Email
            })
            .ToListAsync();

        return Json(clientes);
    }

    // GET: Criar
    public IActionResult Create()
    {
        return View();
    }

    // POST: Criar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        _context.Add(cliente);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Editar
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    // POST: Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(cliente);

        _context.Update(cliente);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    // GET: Delete
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    // POST: Delete confirmado
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return NotFound();

        // 🔒 VERIFICA SE EXISTE VÍNCULO
        bool possuiContasReceber = await _context.ContasReceber
            .AnyAsync(c => c.ClienteId == id);

        bool possuiVendas = await _context.Vendas
            .AnyAsync(v => v.ClienteId == id);

        bool possuiOrcamentos = await _context.Orcamentos
            .AnyAsync(o => o.ClienteId == id);

        // SE EXISTIR VÍNCULO → BLOQUEIA
        if (possuiContasReceber || possuiVendas || possuiOrcamentos)
        {
            TempData["Erro"] =
                "❌ Não é possível excluir este cliente porque existem registros vinculados.";

            return RedirectToAction(nameof(Index));
        }

        _context.Clientes.Remove(cliente);

        await _context.SaveChangesAsync();

        TempData["Sucesso"] =
            "✅ Cliente excluído com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // ================= DETAILS =================
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    // ========================================
    // DADOS PARA MODAL DE EXCLUSÃO
    // ========================================
    [HttpGet]
    public async Task<IActionResult> ObterCliente(int id)
    {
        var cliente = await _context.Clientes
            .Where(c => c.Id == id)
            .Select(c => new
            {
                id = c.Id,
                nome = c.Nome,
                cpfCnpj = c.CpfCnpj,
                telefone = c.Telefone,
                email = c.Email
            })
            .FirstOrDefaultAsync();

        if (cliente == null)
            return NotFound();

        return Json(cliente);
    }
}