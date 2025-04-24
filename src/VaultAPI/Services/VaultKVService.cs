// Ruta: src/VaultAPI/Services/VaultKVService.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;  // Para inyectar IOptions
using System.Collections.Generic;  // Para usar Dictionary

namespace VaultAPI.Services
{
    public class VaultKVService
    {
        private readonly HttpClient _httpClient;
        private readonly string _vaultAddress;
        private readonly VaultIamAuthService _vaultIamAuthService;

        // Constructor para recibir la configuración de Vault y VaultIamAuthService
        public VaultKVService(IOptions<VaultConfig> vaultConfig, VaultIamAuthService vaultIamAuthService)
        {
            _vaultAddress = vaultConfig.Value.VaultAddress.TrimEnd('/');  // Obtener la URL de Vault desde la configuración
            _vaultIamAuthService = vaultIamAuthService;  // Usar el servicio de autenticación
            _httpClient = new HttpClient();
        }

        // Método para obtener el token de Vault
        private async Task<string> GetVaultTokenAsync()
        {
            var token = await _vaultIamAuthService.LoginAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("No se pudo obtener el token de Vault.");
            }
            return token;
        }

        // Método para leer secretos de Vault
        public async Task<string?> ReadSecretAsync(string path)
        {
            var vaultToken = await GetVaultTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vaultToken);

            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";
            var response = await _httpClient.GetAsync(fullPath);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("data")
                      .GetProperty("data")
                      .GetProperty("value")
                      .GetString();
        }

        // Método para escribir secretos en Vault
        public async Task<bool> WriteSecretAsync(string path, string value, int cas = 0)
        {
            var vaultToken = await GetVaultTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vaultToken);

            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";

            var body = new
            {
                options = new { cas = cas },
                data = new { value = value }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(fullPath, content);
            return response.IsSuccessStatusCode;
        }

        // Método para escribir secretos crudos (raw) en Vault
        public async Task<bool> WriteSecretRawAsync(string path, Dictionary<string, object> data, int cas = 0)
        {
            var vaultToken = await GetVaultTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vaultToken);

            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";

            var body = new
            {
                options = new { cas = cas },
                data = data
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(fullPath, content);
            return response.IsSuccessStatusCode;
        }

        // Método para eliminar una versión de un secreto en Vault
        public async Task<bool> DeleteSecretVersionAsync(string path, int version)
        {
            var vaultToken = await GetVaultTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", vaultToken);

            var fullPath = $"{_vaultAddress}/v1/kv/delete/{path}";

            var body = new
            {
                versions = new[] { version }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(fullPath, content);
            return response.IsSuccessStatusCode;
        }
    }
}
