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

## Endpoints principales
Método	Ruta	Descripción	Autenticación
POST	/auth/register	Registrar nuevo usuario	Pública
POST	/auth/login	Iniciar sesión y obtener JWT + RefreshToken	 Pública
POST	/auth/refresh	Renovar el JWT usando un refresh token	 Pública
POST	/auth/logout	Cerrar sesión e invalidar el token actual	 Requiere JWT

## Flujo de autenticación con Refresh Token
El usuario se registra o inicia sesión
La API responde con un accessToken y un refreshToken
El frontend usa el access token hasta que expire (1h)
Cuando expira, envía el refresh token a /auth/refresh
La API valida el refresh token y responde con un nuevo access token
En logout, el access token se invalida (blacklist)

## Pruebas unitarias
Cobertura actual:

Registro de usuario
Login exitoso y fallido
Control de intentos fallidos y bloqueo
Logout y token en blacklist

