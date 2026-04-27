using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers
{
    public class ContasReceberController : Controller
    {
        private readonly AppDbContext _context;

        public ContasReceberController(AppDbContext context)
        {
            _context = context;
        }

        // Listar todas as contas a receber
        public async Task<IActionResult> Index()
        {
            // O Include busca os dados da Venda e do Cliente para aparecer o nome na tela
            var contas = await _context.ContasReceber
                .Include(c => c.Venda)
                .ThenInclude(v => v.Cliente)
                .OrderByDescending(c => c.DataVencimento)
                .ToListAsync();

            return View(contas);
        }

        // Método para dar baixa (pagar)
        [HttpPost]
        public async Task<IActionResult> Baixar(int id)
        {
            var conta = await _context.ContasReceber.FindAsync(id);
            if (conta != null)
            {
                conta.DataPagamento = DateTime.UtcNow; // Marca como pago hoje
                _context.Update(conta);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
