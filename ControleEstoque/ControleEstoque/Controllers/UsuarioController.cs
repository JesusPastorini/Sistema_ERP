using ControleEstoque.Data;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Controllers
{
    // 🔐 SOMENTE ADMIN
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
            CarregarPermissoes();

            return View();
        }

        // ================= CREATE POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            ModelState.Remove("Permissao");

            // EMAIL DUPLICADO
            bool emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    "Email",
                    "Já existe um usuário com este email.");
            }

            if (!ModelState.IsValid)
            {
                CarregarPermissoes(usuario.PermissaoId);

                return View(usuario);
            }

            // HASH SENHA
            usuario.Senha =
                BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            CarregarPermissoes(usuario.PermissaoId);

            // NÃO MOSTRA HASH
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
            ModelState.Remove("ConfirmarSenha");

            var usuarioBanco = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

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
                CarregarPermissoes(usuario.PermissaoId);

                return View(usuario);
            }

            usuarioBanco.Nome = usuario.Nome;
            usuarioBanco.Email = usuario.Email;
            usuarioBanco.PermissaoId = usuario.PermissaoId;

            // ALTERA SENHA SOMENTE SE INFORMAR
            if (!string.IsNullOrWhiteSpace(usuario.Senha))
            {
                usuarioBanco.Senha =
                    BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
            }

            _context.Update(usuarioBanco);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= AUXILIAR =================
        private void CarregarPermissoes(int? selecionado = null)
        {
            ViewBag.PermissaoId = new SelectList(
                _context.Permissoes.OrderBy(p => p.NomePerfil),
                "Id",
                "NomePerfil",
                selecionado
            );
        }

        [HttpGet]
        public async Task<IActionResult> ObterUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Permissao)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    id = u.Id,
                    nome = u.Nome,
                    email = u.Email,
                    perfil = u.Permissao.NomePerfil
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound();

            return Json(usuario);
        }
    }
}