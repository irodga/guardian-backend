using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VaultAPI.Services
{
    public class VaultKVService
    {
        private readonly HttpClient _httpClient;
        private readonly string _vaultAddress;

        // Constructor modificado para aceptar el HttpClient inyectado
        public VaultKVService(HttpClient httpClient, string vaultAddress)
        {
            _vaultAddress = vaultAddress.TrimEnd('/');
            _httpClient = httpClient;
        }

        // Método para leer un secreto
        public async Task<string?> ReadSecretAsync(string path)
        {
            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";

            var response = await _httpClient.GetAsync(fullPath);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al leer el secreto: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            try
            {
                return doc.RootElement
                          .GetProperty("data")
                          .GetProperty("data")
                          .GetProperty("value")
                          .GetString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar la respuesta de Vault: {ex.Message}");
                return null;
            }
        }

        // Método para escribir un secreto
        public async Task<bool> WriteSecretAsync(string path, string value, int cas = 0)
        {
            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";

            var body = new
            {
                options = new { cas = cas },
                data = new { value = value }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(fullPath, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al escribir el secreto: {response.StatusCode}");
            }
            return response.IsSuccessStatusCode;
        }

        // Método para escribir un secreto en bruto
        public async Task<bool> WriteSecretRawAsync(string path, Dictionary<string, object> data, int cas = 0)
        {
            var fullPath = $"{_vaultAddress}/v1/kv/data/{path}";

            var body = new
            {
                options = new { cas = cas },
                data = data
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(fullPath, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al escribir el secreto: {response.StatusCode}");
            }
            return response.IsSuccessStatusCode;
        }

        // Método para eliminar una versión de un secreto
        public async Task<bool> DeleteSecretVersionAsync(string path, int version)
        {
            var fullPath = $"{_vaultAddress}/v1/kv/delete/{path}";

            var body = new
            {
                versions = new[] { version }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(fullPath, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al eliminar la versión del secreto: {response.StatusCode}");
            }
            return response.IsSuccessStatusCode;
        }
    }
}
