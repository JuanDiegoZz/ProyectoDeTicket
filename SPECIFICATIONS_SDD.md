# Especificaciones de Módulos (Specification-Driven Development - SDD)
## Sistema de Gestión de Tickets de Soporte TI — TecNM Campus Monclova

---

### Módulo 1: Autenticación y Clasificación de Correo (Auth & Classifier)
- **Objetivo:** Autenticar usuarios mediante Cookies y clasificar automáticamente el rol y prioridad del solicitante según su correo institucional.
- **Reglas de Negocio:**
  - Alumnos: `^[a-zA-Z]\d{8}@monclova\.tecnm\.mx$` $\rightarrow$ Prioridad Normal.
  - Docentes: `^[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+@monclova\.tecnm\.mx$` $\rightarrow$ Prioridad Alta Automática.
- **Contratos:** `IEmailClassifierService.cs`, `AccountController.cs`.
- **Criterio de Aceptación:** Cualquier registro que no cumpla con los patrones de dominio es rechazado con mensaje de error específico en español.

---

### Módulo 2: Ciclo de Vida del Ticket (Ticket Core)
- **Objetivo:** Gestionar el ciclo completo de una incidencia técnica desde su apertura hasta la calificación de satisfacción.
- **Estados:** `Abierto` $\rightarrow$ `EnProgreso` $\rightarrow$ `Resuelto` (o `Cancelado`).
- **Contratos:** `ITicketService.cs`, `TicketsController.cs`.
- **Criterios de Aceptación:**
  1. La subida de evidencias solo permite archivos `.jpg`, `.jpeg`, `.png`, `.webp`, `.pdf` con nombres UUID únicos.
  2. Cada cambio de estado o reasignación genera automáticamente una entrada en `notas_ticket`.
  3. Los tickets resueltos permiten al solicitante calificar de 1 a 5 estrellas con comentario opcional.

---

### Módulo 3: Catálogos Dinámicos (Catalogs Management)
- **Objetivo:** Permitir al Administrador gestionar categorías de fallas y ubicaciones del campus sin necesidad de modificar código fuente ni reiniciar el servidor.
- **Contratos:** `ICatalogoService.cs`, `CatalogosController.cs`.
- **Criterios de Aceptación:**
  1. Operaciones CRUD completas (Crear, Editar, Consultar, Activar/Desactivar).
  2. Las categorías o ubicaciones desactivadas no aparecen en el formulario de creación de nuevos tickets pero mantienen la integridad histórica en tickets existentes.

---

### Módulo 4: Analítica y Exportación Filtrada (Analytics & Export)
- **Objetivo:** Visualizar métricas en tiempo real y exportar auditorías en formato compatible con Excel respetando los filtros activos.
- **Contratos:** `DashboardAdminViewModel.cs`, `TicketsController.ExportarCsv`.
- **Criterios de Aceptación:**
  1. Las gráficas de Chart.js reflejan las fallas por estado, edificio y categoría.
  2. La exportación CSV incluye Byte Order Mark (UTF-8 BOM) y contiene única y exclusivamente los registros que coinciden con los filtros aplicados en pantalla.
