// src/VaultAPI/Services/VaultKVService.cs
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
        private readonly string _vaultToken;

        public VaultKVService(string vaultAddress, string vaultToken)
        {
            _vaultAddress = vaultAddress.TrimEnd('/');
            _vaultToken = vaultToken;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _vaultToken);
        }

        public async Task<string?> ReadSecretAsync(string path)
        {
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
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSecretVersionAsync(string path, int version)
        {
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
