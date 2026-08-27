# DESIGN.md — Especificación de Arquitectura Desacoplada (Backend REST & Frontend Vue 3)
## Sistema de Gestión de Tickets de Soporte TI — TecNM Campus Monclova

---

### 1. Resumen Ejecutivo y Esquema de Arquitectura
El sistema implementa una arquitectura desacoplada y dividida en dos aplicaciones físicas totalmente independientes: un **Backend API REST en .NET 10 / C#** y un **Frontend SPA en Vue 3**.

```
                 ┌─────────────────────────────────┐
                 │          Vue 3 SPA              │
                 │      (Frontend Client)          │
                 └────────────────┬────────────────┘
                                  │
                             HTTP / JSON
                       (CORS / Credentials)
                                  │
                                  ▼
                 ┌─────────────────────────────────┐
                 │       .NET 10 API REST          │
                 │   (Clean Architecture Backend)  │
                 └────────────────┬────────────────┘
                                  │
                               EF Core
                                  │
                                  ▼
                 ┌─────────────────────────────────┐
                 │  PostgreSQL 18 / SQLite DB      │
                 │          (snake_case)           │
                 └─────────────────────────────────┘
```

---

### 2. Responsabilidades por Capa

#### 2.1. Frontend (Vue 3 SPA)
- **Capa de Vistas (`views/`)**: Renderiza los dashboards, formularios, tablas responsivas y vistas por rol (*Administrador, Técnico, Solicitante*).
- **Capa de Componentes (`components/`)**: Componentes de navegación e interfaz compartida.
- **Capa de Servicios (`services/`)**: Centraliza todas las llamadas `fetch` a los endpoints REST sin dispersar lógica de red en las vistas.
- **Estilos (`assets/styles.css`)**: Soporte nativo para modo claro y modo oscuro.

#### 2.2. Backend (API REST .NET 10)
- **Controladores (`Controllers/`)**: Exponen endpoints REST HTTP con respuestas JSON en `camelCase`.
- **Dominio (`Domain/`)**: Entidades POCO (`Usuario`, `Ticket`, `Categoria`, `Ubicacion`, `NotaTicket`) y Enums.
- **Aplicación (`Application/`)**: Interfaces de servicio, ViewModels/DTOs y paginación en servidor (`PagedResult<T>`).
- **Infraestructura (`Infrastructure/`)**: Contexto EF Core (`ApplicationDbContext`), repositorios de servicio y lógica de negocio.

---

### 3. Matriz de Módulos y Documentación Asociada

| Módulo | Tipo | Ubicación | Documentación .md |
| :--- | :--- | :--- | :--- |
| **API REST Backend** | API | `Backend/API/` | [`Backend/README.md`](file:///c:/Users/juand/Desktop/Tickets/Backend/README.md) |
| **Vue 3 SPA Frontend** | SPA | `Frontend/` | [`Frontend/README.md`](file:///c:/Users/juand/Desktop/Tickets/Frontend/README.md) |
| **Controladores API** | REST | `Backend/API/Controllers/` | Ver Endpoints en `Backend/README.md` |
| **Servicios Frontend** | HTTP | `Frontend/src/services/` | Ver Servicios en `Frontend/README.md` |
| **Modelos de Dominio** | POCO | `Backend/API/Domain/` | [`DESIGN.md`](file:///c:/Users/juand/Desktop/Tickets/DESIGN.md) |
