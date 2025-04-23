using VaultAPI;
using Microsoft.EntityFrameworkCore;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GuardianDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

var app = builder.Build();

// Configurar el middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

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
