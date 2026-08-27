# DESIGN.md — Especificación de Arquitectura y Diseño del Sistema
## Sistema de Gestión de Tickets de Soporte TI — TecNM Campus Monclova

---

### 1. Resumen Ejecutivo y Arquitectura General
El sistema implementa una arquitectura desacoplada basada en **Clean Architecture**, **SOLID** y el patrón **ASP.NET Core MVC** bajo **.NET 10** y **PostgreSQL 18**.

```
                           +------------------------------------------+
                           |           Capa de Presentación           |
                           |   (Controllers, Razor Views, Assets)     |
                           +--------------------+---------------------+
                                                |
                                                v
                           +--------------------+---------------------+
                           |            Capa Application              |
                           |   (Interfaces, ViewModels, PagedResult)  |
                           +--------------------+---------------------+
                                                |
                                                v
                           +--------------------+---------------------+
                           |              Capa Domain                 |
                           |        (Entities, Enums de Negocio)      |
                           +--------------------+---------------------+
                                                ^
                                                |
                           +--------------------+---------------------+
                           |          Capa Infrastructure             |
                           |    (EF Core DbContext, Services, Npgsql) |
                           +------------------------------------------+
```

---

### 2. Estructura de Capas y Responsabilidades

#### 2.1. Capa de Dominio (`Domain/`)
- **`Domain/Entities/`**:
  - `Usuario.cs`: Representa los actores del sistema (Solicitante, Técnico, Administrador).
  - `Ticket.cs`: Entidad central con auditoría, priorización y calificación de servicio (1 a 5 estrellas).
  - `Categoria.cs`: Clasificación dinámica de fallas técnicas.
  - `Ubicacion.cs`: Edificios y áreas del campus.
  - `NotaTicket.cs`: Bitácora cronológica e historial de eventos por incidencia.
- **`Domain/Enums/CommonEnums.cs`**:
  - `RolUsuario`: `Solicitante`, `Tecnico`, `Administrador`.
  - `TipoSolicitante`: `Alumno`, `Profesor`, `Administrativo`, `Desconocido`.
  - `PrioridadTicket`: `Baja`, `Normal`, `Alta`, `Urgente`.
  - `EstadoTicket`: `Abierto`, `EnProgreso`, `Resuelto`, `Cancelado`.

#### 2.2. Capa de Aplicación (`Application/`)
- **`Application/Common/Interfaces/`**:
  - `ITicketService`: Contrato para el ciclo de vida del ticket, filtrado dinámico y paginación en servidor.
  - `ICatalogoService`: Contrato para la administración de categorías y ubicaciones dinámicas.
  - `IEmailClassifierService`: Contrato para la clasificación inteligente de correos por Regex.
- **`Application/Common/Models/`**:
  - `PagedResult<T>`: Contenedor genérico para paginación segura desde base de datos.
- **`Application/ViewModels/`**:
  - `LoginViewModel`, `RegistroViewModel`, `CrearTicketViewModel`, `DashboardAdminViewModel`.

#### 2.3. Capa de Infraestructura (`Infrastructure/`)
- **`Infrastructure/Data/ApplicationDbContext.cs`**:
  - Configuración Fluent API con convención estricta de nombres en `snake_case` para PostgreSQL 18.
  - Índices en `email`, `estado`, `prioridad`, `fecha_creacion` y claves foráneas.
  - Sembrado automático de catálogos y usuario Administrador inicial.
- **`Infrastructure/Services/`**:
  - `TicketService.cs`: Consultas optimizadas con LINQ, `Skip()` y `Take()` para paginación y filtros combinados.
  - `CatalogoService.cs`: Operaciones CRUD y alternancia de alta/baja lógica.
  - `EmailClassifierService.cs`: Motor de expresiones regulares para correos institucionales `@monclova.tecnm.mx`.

#### 2.4. Capa de Presentación (`Controllers/` y `Views/`)
- **`Controllers/`**:
  - `TicketsController.cs`: Flujos de tickets, detalle, cambios de estado, notas, reasignación y exportación CSV con BOM.
  - `AccountController.cs`: Autenticación por Cookies, registro con validación Regex y gestión de técnicos.
  - `CatalogosController.cs`: Administración de categorías y ubicaciones (exclusivo para Administrador).
- **`Views/`**:
  - Vistas Razor responsivas con Bootstrap 5, Chart.js y soporte de Modo Claro / Modo Oscuro.

---

### 3. Reglas de Modificación para Futuros Desarrolladores y Agentes de IA
1. **Regla de Dependencias:** La capa de `Domain` nunca debe referenciar a `Infrastructure` ni a `Controllers`.
2. **Consultas y Paginación:** Toda nueva consulta sobre listados debe implementarse en `ITicketService` / `Infrastructure` utilizando `PagedResult<T>` para evitar sobrecarga de memoria.
3. **Persistencia en Base de Datos:** Los nombres de nuevas tablas, columnas o índices en `ApplicationDbContext` deben seguir el formato `snake_case`.
4. **Seguridad y Roles:** Los endpoints administrativos deben verificar explícitamente el rol de `Administrador` antes de ejecutar cambios en catálogos o personal.
