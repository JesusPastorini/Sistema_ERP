using ControleEstoque.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var port = Environment.GetEnvironmentVariable("PORT");

    if (!string.IsNullOrEmpty(port))
    {
        serverOptions.ListenAnyIP(int.Parse(port));
    }
});

// MVC
builder.Services.AddControllersWithViews();

// DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.CommandTimeout(60);
    })
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
    options.AddPolicy(
        "PodeGerenciarUsuarios",
        policy => policy.RequireClaim(
            "PodeGerenciarUsuarios",
            "True"));

    options.AddPolicy(
        "PodeGerenciarClientes",
        policy => policy.RequireClaim(
            "PodeGerenciarClientes",
            "True"));

    options.AddPolicy(
        "PodeGerenciarEstoque",
        policy => policy.RequireClaim(
            "PodeGerenciarEstoque",
            "True"));

    options.AddPolicy(
        "PodeGerenciarProdutos",
        policy => policy.RequireClaim(
            "PodeGerenciarProdutos",
            "True"));

    options.AddPolicy(
        "PodeGerenciarVendas",
        policy => policy.RequireClaim(
            "PodeGerenciarVendas",
            "True"));

    options.AddPolicy(
        "PodeVerFinanceiro",
        policy => policy.RequireClaim(
            "PodeVerFinanceiro",
            "True"));
});

var app = builder.Build();

// APLICA MIGRATIONS AUTOMATICAMENTE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

// PIPELINE
app.UseDeveloperExceptionPage();
//if (!app.Environment.IsDevelopment())
//{
// app.UseExceptionHandler("/Home/Error");
//  app.UseHsts();
//}

var cultura = new CultureInfo("pt-BR");

CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

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