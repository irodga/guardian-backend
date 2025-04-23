// Program.cs 
using VaultAPI;
using Microsoft.EntityFrameworkCore;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Establece el puerto de escucha
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Leer credenciales de RDS desde Secrets Manager
async Task<Dictionary<string, string>> GetRdsCredentialsAsync(string secretName)
{
    var client = new AmazonSecretsManagerClient();
    var request = new GetSecretValueRequest
    {
        SecretId = secretName
    };

    var response = await client.GetSecretValueAsync(request);
    var secretJson = response.SecretString;

    return JsonSerializer.Deserialize<Dictionary<string, string>>(secretJson);
}

var secrets = await GetRdsCredentialsAsync("guardian-rds-admin-password");
string dbUser = secrets["username"];
string dbPass = secrets["password"];
string connStr = $"Server=guardian-aurora-cluster.cluster-czk5kyvhmiu2.us-east-1.rds.amazonaws.com;Port=3306;Database=guardian;Uid={dbUser};Pwd={dbPass};";

// Configurar servicios
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de autenticación y autorización
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";  // Redirige al login si no está autenticado
        options.AccessDeniedPath = "/AccessDenied";  // Ruta cuando el acceso es denegado
    });

builder.Services.AddDbContext<GuardianDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

var app = builder.Build();

// Configurar el middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Rutas y configuración de autenticación
app.UseRouting(); // Asegura que las rutas estén configuradas antes de autenticación
app.UseAuthentication();  // Asegura que se use la autenticación
app.UseAuthorization();   // Asegura que se use la autorización

app.UseStaticFiles();  // Archivos estáticos como CSS, JS, imágenes, etc.

// Rutas de controladores
app.MapControllerRoute(
    name: "login",
    pattern: "Login/Index", 
    defaults: new { controller = "Login", action = "Index" });
    
app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/Index", 
    defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "secrets",
    pattern: "Secrets/Index", 
    defaults: new { controller = "Secrets", action = "Index" });

app.MapControllerRoute(
    name: "secretCreate",
    pattern: "Secrets/Create", 
    defaults: new { controller = "Secrets", action = "Create" });

app.MapControllerRoute(
    name: "secretView",
    pattern: "Secrets/View/{id:int}",
    defaults: new { controller = "Secrets", action = "View" });

app.MapControllerRoute(
    name: "secretDelete",
    pattern: "Secrets/Delete/{id:int}",
    defaults: new { controller = "Secrets", action = "Delete" });

app.MapControllerRoute(
    name: "login",
    pattern: "Login/Index", 
    defaults: new { controller = "Login", action = "Index" });

app.MapControllerRoute(
    name: "accessDenied",
    pattern: "AccessDenied", 
    defaults: new { controller = "AccessDenied", action = "Index" });

app.MapControllerRoute(
    name: "users",
    pattern: "Users/Index", 
    defaults: new { controller = "Users", action = "Index" });

app.MapControllerRoute(
    name: "companies",
    pattern: "Companies/Index", 
    defaults: new { controller = "Companies", action = "Index" });

app.MapControllerRoute(
    name: "groups",
    pattern: "Groups/Index", 
    defaults: new { controller = "Groups", action = "Index" });

app.MapControllers(); // Esto configura las rutas de los controladores

await TestVaultLogin();
app.Run();

// Función de prueba para autenticación con Vault
static async Task TestVaultLogin()
{
    var vaultService = new VaultIamAuthService();
    var token = await vaultService.LoginAsync();

    if (!string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine($"Vault token obtenido: {token}");
    }
    else
    {
        Console.WriteLine("Falló el login IAM con Vault.");
    }
}
