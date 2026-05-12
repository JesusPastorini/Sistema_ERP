using ControleEstoque.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention()
);

// AUTHENTICATION
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/AcessoNegado";
    });

// AUTHORIZATION
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PodeGerenciarUsuarios",
        policy => policy.RequireClaim(
            "PodeGerenciarUsuarios", "True"));

    options.AddPolicy("PodeVerFinanceiro",
        policy => policy.RequireClaim(
            "PodeVerFinanceiro", "True"));

    options.AddPolicy("PodeGerenciarEstoque",
        policy => policy.RequireClaim(
            "PodeGerenciarEstoque", "True"));
});

var app = builder.Build();

// PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ROTA PADRÃO
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();