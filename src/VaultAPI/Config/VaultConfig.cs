// Ruta: src/VaultAPI/Config/VaultConfig.cs

namespace VaultAPI
{
    public class VaultConfig
    {
        public string VaultAddress { get; set; } = string.Empty;  // Dirección de Vault
        public string VaultToken { get; set; } = string.Empty;    // Si alguna vez decides almacenar el token aquí
    }
}
