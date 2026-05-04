using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers
{
    public class CondicoesPagamentoController : Controller
    {
        private readonly AppDbContext _context;

        public CondicoesPagamentoController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 LISTA
        public async Task<IActionResult> Index()
        {
            var lista = await _context.CondicoesPagamento
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return View(lista);
        }

        // 🔹 CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CondicaoPagamento model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Ativo = true; // garante padrão

            _context.CondicoesPagamento.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 🔹 DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var condicao = await _context.CondicoesPagamento
                .FirstOrDefaultAsync(c => c.Id == id);

            if (condicao == null)
                return NotFound();

            return View(condicao);
        }

        // 🔹 EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var condicao = await _context.CondicoesPagamento.FindAsync(id);

            if (condicao == null)
                return NotFound();

            return View(condicao);
        }

        // 🔹 EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CondicaoPagamento model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _context.CondicoesPagamento.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CondicaoExists(model.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var condicao = await _context.CondicoesPagamento
                .FirstOrDefaultAsync(c => c.Id == id);

            if (condicao == null)
                return NotFound();

            return View(condicao);
        }

        // 🔹 DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var condicao = await _context.CondicoesPagamento.FindAsync(id);

            if (condicao != null)
            {
                _context.CondicoesPagamento.Remove(condicao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔹 MÉTODO AUXILIAR
        private bool CondicaoExists(int id)
        {
            return _context.CondicoesPagamento.Any(e => e.Id == id);
        }
    }
}