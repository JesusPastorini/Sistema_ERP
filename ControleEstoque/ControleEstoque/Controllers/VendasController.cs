using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Data;
using ControleEstoque.Models;
using System.Security.Claims;

namespace ControleEstoque.Controllers
{
    public class VendasController : Controller
    {
        private readonly AppDbContext _context;

        public VendasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vendas = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
            return View(vendas);
        }

        public IActionResult Create()
        {
            ViewBag.ClienteId = new SelectList(_context.Set<Cliente>(), "Id", "Nome");
            ViewBag.ProdutoId = new SelectList(_context.Produtos, "Id", "TipoMadeira");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venda venda, int[] ProdutoId, decimal[] Quantidade, decimal[] PrecoUnitario)
        {
            // 1. Identificação do Usuário
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            venda.UsuarioId = string.IsNullOrEmpty(userIdClaim) ? (await _context.Usuarios.FirstAsync()).Id : int.Parse(userIdClaim);
            venda.DataVenda = DateTime.UtcNow;

            // 2. VALIDAÇÃO DE ESTOQUE (Antes de salvar qualquer coisa)
            for (int i = 0; i < ProdutoId.Length; i++)
            {
                var pId = ProdutoId[i];
                var qtdSolicitada = Quantidade[i];
                var produto = await _context.Produtos.FindAsync(pId);

                if (produto == null || produto.QuantidadeEstoque < qtdSolicitada)
                {
                    ModelState.AddModelError("", $"Estoque insuficiente para: {(produto?.TipoMadeira ?? "Produto não encontrado")}. Disponível: {(produto?.QuantidadeEstoque ?? 0)}");

                    ViewBag.ClienteId = new SelectList(_context.Set<Cliente>(), "Id", "Nome", venda.ClienteId);
                    ViewBag.ProdutoId = new SelectList(_context.Produtos, "Id", "TipoMadeira");
                    return View(venda);
                }
            }

            // 3. SALVAR A VENDA
            ModelState.Remove("Usuario");
            ModelState.Remove("Cliente");

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            decimal totalCalculado = 0; // Nome alterado para evitar conflito

            // 4. PROCESSAR ITENS E BAIXAR ESTOQUE
            for (int i = 0; i < ProdutoId.Length; i++)
            {
                var pId = ProdutoId[i];
                var qtd = Quantidade[i];
                var preco = PrecoUnitario[i];

                // Registrar o Item da Venda
                var item = new VendaItem
                {
                    VendaId = venda.Id,
                    ProdutoId = pId,
                    Quantidade = qtd,
                    PrecoUnitario = preco
                };
                _context.VendaItens.Add(item);

                // Baixar o estoque fisicamente
                var produtoEstoque = await _context.Produtos.FindAsync(pId);
                if (produtoEstoque != null)
                {
                    produtoEstoque.QuantidadeEstoque -= qtd;
                }

                totalCalculado += (qtd * preco);
            }

            // 5. ATUALIZAR VALOR FINAL E GERAR FINANCEIRO
            venda.ValorTotal = totalCalculado;

            var contaReceber = new ContasReceber
            {
                VendaId = venda.Id,
                Valor = totalCalculado,
                DataVencimento = DateTime.UtcNow.AddDays(30),
                Observacao = $"Venda ref. #{venda.Id}"
            };

            _context.ContasReceber.Add(contaReceber);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
