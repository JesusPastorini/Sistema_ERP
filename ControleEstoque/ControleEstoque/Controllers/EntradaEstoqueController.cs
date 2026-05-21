using ControleEstoque.Data;
using ControleEstoque.Models;
using ControleEstoque.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ControleEstoque.Controllers
{
    public class EntradaEstoqueController : Controller
    {
        private readonly AppDbContext _context;

        public EntradaEstoqueController(AppDbContext context)
        {
            _context = context;
        }

        // INDEX
        public async Task<IActionResult> Index()
        {
            var entradas = await _context.EntradasEstoque
                .Include(e => e.Fornecedor)
                .OrderByDescending(e => e.DataEntrada)
                .ToListAsync();

            return View(entradas);
        }

        // CREATE GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Fornecedores = new SelectList(
                _context.Fornecedores.OrderBy(f => f.NomeFantasia),
                "Id",
                "NomeFantasia"
            );

            ViewBag.Produtos = new SelectList(
                _context.Produtos.OrderBy(p => p.TipoMadeira),
                "Id",
                "TipoMadeira"
            );

            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    EntradaEstoque entrada,
    int[] ProdutoId,
    decimal[] Quantidade,
    decimal[] ValorCusto)
        {
            ModelState.Remove("Fornecedor");
            ModelState.Remove("Itens");

            if (ProdutoId == null || ProdutoId.Length == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Adicione ao menos um produto."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(entrada);
            }

            entrada.DataEntrada = DateTime.Now;

            _context.EntradasEstoque.Add(entrada);

            await _context.SaveChangesAsync();

            for (int i = 0; i < ProdutoId.Length; i++)
            {
                var produto =
                    await _context.Produtos
                    .FindAsync(ProdutoId[i]);

                if (produto == null)
                    continue;

                decimal qtd =
                    Quantidade[i];

                decimal custo =
                    ValorCusto[i];

                // =====================================
                // ITEM ENTRADA
                // =====================================

                var item =
                    new EntradaEstoqueItem
                    {
                        EntradaEstoqueId = entrada.Id,

                        ProdutoId = produto.Id,

                        Quantidade = qtd,

                        ValorCusto = custo
                    };

                _context.EntradaEstoqueItens.Add(item);

                // =====================================
                // ATUALIZA ESTOQUE
                // =====================================

                produto.QuantidadeEstoque += qtd;

                // =====================================
                // MOVIMENTAÇÃO
                // =====================================

                _context.MovimentacoesEstoque.Add(
                    new MovimentacaoEstoque
                    {
                        ProdutoId = produto.Id,

                        Quantidade = qtd,

                        TipoMovimentacao = "Entrada",

                        DataMovimentacao = DateTime.Now,

                        Descricao =
                            "Entrada manual de estoque"
                    }
                );
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Buscar Produto Paginado
        [HttpGet]
        public async Task<IActionResult> BuscarProdutosPaginado(
    string termo,
    int pagina = 1)
        {
            int itensPorPagina = 10;

            var consulta = _context.Produtos
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                termo = termo.ToLower();

                consulta = consulta.Where(p =>
                    p.Categoria.ToLower().Contains(termo) ||
                    p.TipoMadeira.ToLower().Contains(termo) ||
                    p.Dimensoes.ToLower().Contains(termo));
            }

            var total = await consulta.CountAsync();

            var produtos = await consulta
                .OrderBy(p => p.TipoMadeira)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(p => new
                {
                    id = p.Id,
                    text = p.Categoria + " - " +
                           p.TipoMadeira + " (" +
                           p.Dimensoes + ")"
                })
                .ToListAsync();

            return Json(new
            {
                items = produtos,
                pagination = new
                {
                    more = (pagina * itensPorPagina) < total
                }
            });
        }

        // Buscar Fornecedor Paginado
        [HttpGet]
        public async Task<IActionResult> BuscarFornecedoresPaginado(
     string termo,
     int pagina = 1)
        {
            int itensPorPagina = 10;

            var consulta = _context.Fornecedores
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                termo = termo.ToLower();

                consulta = consulta.Where(f =>
                    f.RazaoSocial.ToLower().Contains(termo));
            }

            var total = await consulta.CountAsync();

            var fornecedores = await consulta
                .OrderBy(f => f.RazaoSocial)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(f => new
                {
                    id = f.Id,
                    text = f.RazaoSocial
                })
                .ToListAsync();

            return Json(new
            {
                items = fornecedores,
                pagination = new
                {
                    more = (pagina * itensPorPagina) < total
                }
            });
        }

        // XML
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportarXml(IFormFile xmlFile)
        {
            try
            {
                if (xmlFile == null || xmlFile.Length == 0)
                {
                    return Content("Arquivo inválido.");
                }

                string xmlContent;

                using (var reader = new StreamReader(
                    xmlFile.OpenReadStream(),
                    System.Text.Encoding.UTF8,
                    true))
                {
                    xmlContent = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    return Content("XML vazio.");
                }

                // REMOVE caracteres inválidos
                xmlContent = xmlContent.Trim();

                XDocument xml = XDocument.Parse(xmlContent);

                XNamespace ns =
                    "http://www.portalfiscal.inf.br/nfe";

                var infNFe = xml
                    .Descendants(ns + "infNFe")
                    .FirstOrDefault();

                if (infNFe == null)
                {
                    return Content("Estrutura NF-e inválida.");
                }

                // =========================
                // FORNECEDOR
                // =========================

                var emit = infNFe.Element(ns + "emit");

                string fornecedorNome =
                    emit?.Element(ns + "xNome")?.Value ?? "";

                // =========================
                // NF
                // =========================

                string numeroNF =
                    infNFe
                    .Element(ns + "ide")?
                    .Element(ns + "nNF")?
                    .Value ?? "";

                // =========================
                // TOTAL
                // =========================

                decimal valorTotal = decimal.Parse(
                    infNFe
                    .Element(ns + "total")?
                    .Element(ns + "ICMSTot")?
                    .Element(ns + "vNF")?
                    .Value ?? "0",
                    System.Globalization.CultureInfo.InvariantCulture
                );

                // =========================
                // PRODUTOS XML
                // =========================

                var produtosXml = infNFe
                    .Descendants(ns + "det")
                    .Select(x => new
                    {
                        Nome = x
                            .Element(ns + "prod")?
                            .Element(ns + "xProd")?
                            .Value ?? "",

                        Quantidade = decimal.Parse(
                            x.Element(ns + "prod")?
                            .Element(ns + "qCom")?
                            .Value ?? "0",
                            System.Globalization.CultureInfo.InvariantCulture
                        ),

                        Valor = decimal.Parse(
                            x.Element(ns + "prod")?
                            .Element(ns + "vUnCom")?
                            .Value ?? "0",
                            System.Globalization.CultureInfo.InvariantCulture
                        )
                    })
                    .ToList();

                // =========================
                // PRODUTOS SISTEMA
                // =========================

                var produtosSistema =
                    await _context.Produtos.ToListAsync();

                // =========================
                // RELACIONAR PRODUTOS
                // =========================

                var itens = produtosXml.Select(p =>
                {
                    var encontrado = produtosSistema
                        .FirstOrDefault(prod =>
                            p.Nome.ToLower()
                            .Contains(prod.TipoMadeira.ToLower()));

                    return new EntradaEstoqueItemViewModel
                    {
                        NomeXml = p.Nome,
                        ProdutoId = encontrado?.Id,
                        Quantidade = p.Quantidade,
                        ValorCusto = p.Valor
                    };
                }).ToList();

                // =========================
                // FORNECEDOR SISTEMA
                // =========================

                var fornecedor =
                    await _context.Fornecedores
                    .FirstOrDefaultAsync(f =>
                        fornecedorNome.ToLower()
                        .Contains(f.NomeFantasia.ToLower()));

                // =========================
                // VIEW MODEL
                // =========================

                var model =
                    new EntradaEstoqueImportacaoViewModel
                    {
                        NumeroNotaFiscal = numeroNF,
                        NomeFornecedor = fornecedorNome,
                        FornecedorId = fornecedor?.Id,
                        ValorTotal = valorTotal,
                        Itens = itens
                    };

                ViewBag.Fornecedores = new SelectList(
                    _context.Fornecedores,
                    "Id",
                    "NomeFantasia"
                );

                ViewBag.Produtos = new SelectList(
                    _context.Produtos,
                    "Id",
                    "TipoMadeira"
                );

                return View("RevisarXml", model);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        // SALVAR IMPORTAÇÃO XML
        [HttpPost]
        public async Task<IActionResult> SalvarImportacao(
            EntradaEstoqueImportacaoViewModel model)
        {
            if (model.Itens == null || !model.Itens.Any())
            {
                TempData["Erro"] = "Nenhum item encontrado.";
                return RedirectToAction(nameof(Create));
            }

            // CRIA ENTRADA
            var entrada = new EntradaEstoque
            {
                DataEntrada = DateTime.Now,
                TipoEntrada = "NotaFiscal",
                NumeroNotaFiscal = model.NumeroNotaFiscal,
                FornecedorId = model.FornecedorId,
                ValorTotal = model.ValorTotal,
                Observacao = "Importado via XML"
            };

            _context.EntradasEstoque.Add(entrada);

            await _context.SaveChangesAsync();

            // SALVA ITENS
            foreach (var item in model.Itens)
            {
                if (item.ProdutoId == null)
                    continue;

                decimal subtotal =
                    item.Quantidade * item.ValorCusto;

                var entradaItem = new EntradaEstoqueItem
                {
                    EntradaEstoqueId = entrada.Id,
                    ProdutoId = item.ProdutoId.Value,
                    Quantidade = item.Quantidade,
                    ValorCusto = item.ValorCusto,
                    Subtotal = subtotal
                };

                _context.EntradaEstoqueItens
                    .Add(entradaItem);

                // ATUALIZA ESTOQUE
                var produto = await _context.Produtos
                    .FindAsync(item.ProdutoId.Value);

                if (produto != null)
                {
                    produto.QuantidadeEstoque += item.Quantidade;
                }

                // MOVIMENTAÇÃO
                _context.MovimentacoesEstoque.Add(
                    new MovimentacaoEstoque
                    {
                        ProdutoId = item.ProdutoId.Value,
                        Quantidade = item.Quantidade,
                        TipoMovimentacao = "Entrada",
                        DataMovimentacao = DateTime.Now,
                        Descricao =
                            $"Entrada XML NF {model.NumeroNotaFiscal}"
                    });
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                "Entrada importada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
    }
}