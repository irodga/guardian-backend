var builder = WebApplication.CreateBuilder(args);

// Establece el puerto de escucha
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ⚠️ Opcional si no usas HTTPS directamente
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 👉 Llamamos al login de Vault para pruebas
await TestVaultLogin(); // <- solo esta línea
app.Run();

static async Task TestVaultLogin()
{
    var vaultService = new VaultIamAuthService();
    var token = await vaultService.LoginAsync();

    if (!string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine($"✅ Vault token obtenido: {token}");
    }
    else
    {
        Console.WriteLine("❌ Falló el login IAM con Vault.");
    }
}
