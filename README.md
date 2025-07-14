# AuthService - Microservicio de Autenticación con JWT

Este microservicio fue desarrollado como parte de una prueba técnica para la posición de Desarrollador Backend. Implementa un sistema completo de autenticación de usuarios utilizando JWT (JSON Web Tokens) con soporte para refresh tokens, blacklist de tokens comprometidos y control de sesiones.

---

## 🧱 Tecnologías utilizadas

- ASP.NET Core 8
- Entity Framework Core
- SQL Server (Azure SQL)
- JWT (System.IdentityModel.Tokens.Jwt)
- xUnit + Moq (pruebas unitarias)

Decisiones técnicas
JWT Config
Key: Clave secreta para firmar tokens
Issuer: Emisor del token (AuthService)
Audience: Cliente previsto (AuthServiceClient)
Estas validaciones están activadas para mayor seguridad en producción.

 Arquitectura
Se usó arquitectura por capas:
Domain: Entidades e interfaces
Application: Servicios y lógica de negocio
Infrastructure: Persistencia de datos
API: Controladores y configuración web
Se aplicó el patrón Repository 

