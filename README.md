# 📦 Guardian Backend

Este proyecto es el backend del sistema **Guardian**, encargado de gestionar accesos seguros a secretos empresariales, con integración a Vault, control por grupos/empresas, y auditoría detallada.

## 📚 Estructura

- `guardian-backend.sln`: Solución principal
- `src/VaultAPI/`: Proyecto principal con API, modelos y servicios

## 🛠 Requisitos

- .NET 8 SDK
- Aurora MySQL
- AWS Secrets Manager (para credenciales de base)

## 🧾 Esquema de Base de Datos (`guardian`)

```sql
-- 👤 Usuarios del sistema
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255),
    auth_type ENUM('local', 'saml') NOT NULL DEFAULT 'local',
    is_admin BOOLEAN NOT NULL DEFAULT FALSE
);

-- 🏢 Grupos empresariales
CREATE TABLE groups (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

-- 🏭 Empresas dentro de grupos
CREATE TABLE companies (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    group_id INT NOT NULL,
    FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE
);

-- 🔐 Secretos protegidos por Vault
CREATE TABLE secrets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(20) NOT NULL,
    vault_path VARCHAR(255) NOT NULL,
    expiration DATETIME NULL,
    requires_approval BOOLEAN NOT NULL DEFAULT FALSE,
    company_id INT NOT NULL,
    FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE CASCADE
);

-- 🔑 Accesos a secretos con permisos por usuario
CREATE TABLE secret_accesses (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    secret_id INT NOT NULL,
    permission VARCHAR(20) NOT NULL DEFAULT 'read',
    granted_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (secret_id) REFERENCES secrets(id) ON DELETE CASCADE
);

-- 📜 Logs de auditoría de accesos y acciones sobre secretos
CREATE TABLE secret_audit_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    secret_id INT NOT NULL,
    action VARCHAR(20) NOT NULL,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    details TEXT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (secret_id) REFERENCES secrets(id) ON DELETE CASCADE
);

-- ✅ Notas:
-- - Usuarios pueden ser locales o federados (campo 'auth_type')
-- - Secretos se agrupan por empresa y grupo
-- - Permisos definidos por usuario/secreto (tabla 'secret_accesses')
-- - Auditoría detallada en 'secret_audit_logs'

