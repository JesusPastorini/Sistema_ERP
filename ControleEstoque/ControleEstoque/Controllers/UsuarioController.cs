using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{
    [Authorize(Policy = "PodeGerenciarUsuarios")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index(string busca)
        {
            var query = _context.Usuarios
                .Include(u => u.Permissao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.ToLower();

                query = query.Where(u =>
                    u.Nome.ToLower().Contains(busca) ||
                    u.Email.ToLower().Contains(busca));
            }

            var usuarios = await query
                .OrderBy(u => u.Nome)
                .ToListAsync();

            ViewBag.Busca = busca;

            return View(usuarios);
        }

        // ================= CREATE GET =================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PermissaoId = new SelectList(
                _context.Permissoes.OrderBy(p => p.NomePerfil),
                "Id",
                "NomePerfil"
            );

            return View();
        }

        // ================= CREATE POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            ModelState.Remove("Permissao");

            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Já existe um usuário com este email.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PermissaoId = new SelectList(
                    _context.Permissoes.OrderBy(p => p.NomePerfil),
                    "Id",
                    "NomePerfil",
                    usuario.PermissaoId
                );

                return View(usuario);
            }

            // HASH DA SENHA
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            ViewBag.PermissaoId = new SelectList(
                _context.Permissoes.OrderBy(p => p.NomePerfil),
                "Id",
                "NomePerfil",
                usuario.PermissaoId
            );

            // NÃO EXIBE HASH DA SENHA
            usuario.Senha = "";

            return View(usuario);
        }

        // ================= EDIT POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id)
                return NotFound();

            ModelState.Remove("Permissao");

            var usuarioBanco = await _context.Usuarios.FindAsync(id);

            if (usuarioBanco == null)
                return NotFound();

            // EMAIL DUPLICADO
            bool emailExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Email == usuario.Email &&
                    u.Id != usuario.Id);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    "Email",
                    "Já existe um usuário com este email.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PermissaoId = new SelectList(
                    _context.Permissoes.OrderBy(p => p.NomePerfil),
                    "Id",
                    "NomePerfil",
                    usuario.PermissaoId
                );

                return View(usuario);
            }

            usuarioBanco.Nome = usuario.Nome;
            usuarioBanco.Email = usuario.Email;
            usuarioBanco.PermissaoId = usuario.PermissaoId;

            // ALTERA SENHA SOMENTE SE PREENCHER
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                usuarioBanco.Senha =
                    BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}