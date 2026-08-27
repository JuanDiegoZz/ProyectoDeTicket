# Backend - API REST .NET 10 (TecNM Soporte TI)

## 1. Propósito del Módulo
El Backend se encarga **exclusivamente** de la lógica de negocio, acceso a datos en PostgreSQL 18 / SQLite, autenticación basada en Cookies de sesión, reglas de prioridad por correo institucional (`@monclova.tecnm.mx`) y la exposición de endpoints HTTP / JSON para el Frontend Vue 3.

---

## 2. Estructura de Directorios
```
Backend/API/
├── Controllers/
│   ├── AuthController.cs
│   ├── CatalogosController.cs
│   ├── AccountController.cs
│   └── TicketsController.cs
├── Domain/
│   ├── Entities/ (Usuario, Ticket, Categoria, Ubicacion, NotaTicket)
│   └── Enums/ (CommonEnums.cs)
├── Application/
│   ├── Common/Interfaces/ (ITicketService, ICatalogoService, IEmailClassifierService)
│   ├── Common/Models/ (PagedResult.cs)
│   └── ViewModels/ (DTOs de entrada y salida)
├── Infrastructure/
│   ├── Data/ (ApplicationDbContext.cs)
│   └── Services/ (TicketService, CatalogoService, EmailClassifierService)
└── Program.cs
```

---

## 3. Endpoints REST Expuestos

### 3.1. Autenticación (`/api/auth`)
- **`POST /api/auth/login`**: Inicia sesión y genera cookie de autenticación.
- **`POST /api/auth/registro`**: Registro de usuarios con clasificación automática.
- **`POST /api/auth/logout`**: Cierra la sesión activa.
- **`GET /api/auth/me`**: Devuelve los datos del usuario autenticado.

### 3.2. Catálogos (`/api/catalogos`)
- **`GET /api/catalogos/categorias?soloActivas={bool}`**: Lista categorías.
- **`POST /api/catalogos/categorias`**: Crea nueva categoría (Admin).
- **`PUT /api/catalogos/categorias/{id}`**: Edita categoría existente (Admin).
- **`POST /api/catalogos/categorias/{id}/alternar-estado`**: Alta/Baja lógica de categoría (Admin).
- **`GET /api/catalogos/ubicaciones?soloActivas={bool}`**: Lista ubicaciones.
- **`POST /api/catalogos/ubicaciones`**: Crea nueva ubicación (Admin).
- **`PUT /api/catalogos/ubicaciones/{id}`**: Edita ubicación (Admin).
- **`POST /api/catalogos/ubicaciones/{id}/alternar-estado`**: Alta/Baja lógica de ubicación (Admin).

### 3.3. Gestión de Técnicos (`/api/account`)
- **`GET /api/account/tecnicos`**: Lista el personal técnico (Admin).
- **`POST /api/account/crear-tecnico`**: Registra nuevo técnico (Admin).
- **`POST /api/account/tecnicos/{id}/alternar-estado`**: Modifica estado activo/inactivo (Admin).

### 3.4. Tickets (`/api/tickets`)
- **`GET /api/tickets`**: Consulta paginada y filtrada en servidor.
- **`GET /api/tickets/dashboard-admin`**: Métricas analíticas completas para Chart.js.
- **`GET /api/tickets/{id}`**: Detalle completo del ticket con notas.
- **`POST /api/tickets`**: Crea reporte de falla (FormData con archivo opcional).
- **`POST /api/tickets/{id}/cambiar-estado`**: Actualiza estado (Técnico/Admin).
- **`POST /api/tickets/{id}/reasignar`**: Reasigna técnico (Admin).
- **`POST /api/tickets/{id}/cambiar-prioridad`**: Cambia prioridad (Admin).
- **`POST /api/tickets/{id}/calificar`**: Califica servicio (1 a 5 estrellas).
- **`POST /api/tickets/{id}/notas`**: Agrega comentario a la bitácora.
- **`GET /api/tickets/exportar-csv`**: Descarga CSV con UTF-8 BOM.

---

## 4. Ejecución del Backend
```bash
cd Backend/API
dotnet run --urls "http://localhost:5000"
```
