// src/VaultAPI/Models/SecretAuditLog.cs
namespace VaultAPI.Models
{
    public class SecretAuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Action { get; set; }  // Esta podría ser la acción, por ejemplo "read", "write"
        public DateTime Timestamp { get; set; }  // Esto es lo que usarás para mostrar la fecha
        // Otras propiedades que puedas necesitar
        public Secret Secret { get; set; } = null!;  // Referencia al secreto
    }
}