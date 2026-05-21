using ControleEstoque.Data;
using ControleEstoque.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Today;

            var model = new DashboardViewModel
            {
                // ESTOQUE
                TotalProdutos = await _context.Produtos.CountAsync(),

                ProdutosBaixoEstoque = await _context.Produtos
                    .CountAsync(p => p.QuantidadeEstoque <= 5),

                // MOVIMENTAÇÃO
                SaidasHoje = await _context.MovimentacoesEstoque
                    .CountAsync(s => s.DataMovimentacao.Date == hoje),

                // FINANCEIRO
                ContasReceberHoje = await _context.ContasReceber
                    .Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento.Date == hoje)
                    .SumAsync(c => (decimal?)c.Valor) ?? 0,

                ContasPagarHoje = await _context.ContasPagar
                    .Where(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento.Date == hoje)
                    .SumAsync(c => (decimal?)c.Valor) ?? 0,

                FluxoMes =
                    (
                        await _context.ContasReceber
                            .Where(c =>
                                c.DataPagamento != null &&
                                c.DataPagamento.Value.Month == hoje.Month)
                            .SumAsync(c => (decimal?)c.Valor) ?? 0
                    )
                    -
                    (
                        await _context.ContasPagar
                            .Where(c =>
                                c.DataPagamento != null &&
                                c.DataPagamento.Value.Month == hoje.Month)
                            .SumAsync(c => (decimal?)c.Valor) ?? 0
                    ),

                // COMERCIAL
                VendasMes = await _context.Vendas
                    .CountAsync(v => v.DataVenda.Month == hoje.Month),

                OrcamentosPendentes = await _context.Orcamentos
                    .CountAsync(),

                // ALERTAS
                ContasVencidas = await _context.ContasPagar
                    .CountAsync(c =>
                        c.DataPagamento == null &&
                        c.DataVencimento < hoje),

                ProdutosCriticos = await _context.Produtos
                    .CountAsync(p => p.QuantidadeEstoque <= 2)
            };

            return View(model);
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}