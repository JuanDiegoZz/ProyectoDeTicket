# DOCUMENTACIÓN COMPLETA Y ARTÍCULO CIENTÍFICO TÉCNICO
## Sistema Inteligente de Gestión de Incidencias de Soporte TI basado en Arquitectura Desacoplada (.NET 10 REST API + Vue 3 SPA + PostgreSQL 18)

**Autor:** Equipo de Desarrollo de Software / Dirección de Tecnologías de la Información  
**Institución:** Instituto Tecnológico Superior de Monclova (TecNM Campus Monclova)  
**Fecha:** Agosto de 2026  

---

### RESUMEN (ABSTRACT)
El presente artículo técnico documenta la concepción, desacoplamiento arquitectónico, diseño e implementación del **Sistema de Gestión de Tickets de Soporte TI** del TecNM Campus Monclova. La solución transicionó de un modelo monolítico hacia una **arquitectura desacoplada de alto rendimiento** comprendida por una API REST pura construida en **.NET 10**, una aplicación de página única (**SPA**) en **Vue 3**, y un motor relacional en **PostgreSQL 18**. La plataforma incorpora un clasificador automático de correos institucionales, visualización de métricas en tiempo real con **`vue-chartjs` y `Chart.js`**, sistema unificado de filtros por fecha, edificio y categoría, bitácora de seguimiento tipo *Timeline*, notificaciones flotantes reactivas (`Toasts`), exportación a Excel (CSV con BOM UTF-8) e impresión de fichas oficiales en formato PDF.

**Palabras Clave:** .NET 10, Vue 3, PostgreSQL 18, API REST, Chart.js, vue-chartjs, Soporte TI, SDD, Clean Architecture.

---

## 1. INTRODUCCIÓN Y PLANTEAMIENTO DEL PROBLEMA
En las instituciones de educación superior tecnológica, la continuidad operativa del equipamiento informático (computadoras en laboratorios, proyectores en aulas, redes Wi-Fi/Ethernet y cuentas institucionales) es crítica para el desarrollo académico. 

El modelo tradicional de recepción de fallas mediante mensajes verbales o correos no estructurados generaba:
1. **Falta de visibilidad de tiempos de atención (SLA)**.
2. **Imposibilidad de identificar edificios con mayor tasa de fallas**.
3. **Ausencia de un registro histórico para auditoría**.

Para solucionar esta problemática, se estableció como objetivo diseñar e implementar una plataforma web moderna, segura, responsiva y dividida físicamente en servicios Backend y Frontend.

---

## 2. ARQUITECTURA DEL SISTEMA Y TECNOLOGÍAS

### 2.1. Diagrama de la Arquitectura Desacoplada
```
                       ┌────────────────────────────────────────┐
                       │          Navegador Cliente             │
                       │          Vue 3 SPA (HTML/JS)           │
                       │     (vue-chartjs + Chart.js + CSS)    │
                       └───────────────────┬────────────────────┘
                                           │
                                    HTTP / REST JSON
                             Cookies Sesión (SameSite=Lax)
                                           │
                                           ▼
                       ┌────────────────────────────────────────┐
                       │           Backend API REST             │
                       │       .NET 10 (Clean Architecture)     │
                       │   Controllers | Services | ViewModels  │
                       └───────────────────┬────────────────────┘
                                           │
                                Entity Framework Core
                                           │
                                           ▼
                       ┌────────────────────────────────────────┐
                       │         PostgreSQL 18 Database         │
                       │       Tablas snake_case e Índices       │
                       └────────────────────────────────────────┘
```

### 2.2. Tecnologías Empleadas
- **Backend API**: .NET 10 SDK, ASP.NET Core Web API, BCrypt.Net-Next (hashing de contraseñas), Cookie Authentication (`TecNMTicketsAuthCookie`).
- **Frontend SPA**: Vue 3 (Composition API / Options API), `vue-chartjs` 5.3 + `Chart.js` 4.4, Bootstrap 5.3, Bootstrap Icons 1.11, Tipografía Secundaria Institucional **Montserrat** e Identidad Visual **TecNM (Pantone 294 C / Cool Gray 10 C)**.
- **Base de Datos**: PostgreSQL 18 en entorno Dockerizado / SQLite en entorno de desarrollo.

---

## 3. MÓDULOS DE NEGOCIO E IMPLEMENTACIÓN TÉCNICA

### 3.1. Clasificador Automático de Prioridad por Email Institucional
El servicio `EmailClassifierService.cs` analiza el dominio y prefijo del correo del usuario para determinar su nivel jerárquico y asignar de forma autónoma la prioridad del ticket:
- `@monclova.tecnm.mx` (Jefaturas / Dirección) $\rightarrow$ **Prioridad Alta**.
- Cuentas de alumnos / docentes $\rightarrow$ **Prioridad Normal**.

### 3.2. Dashboard Analítico de Administración y Gráficas
El componente [`DashboardAdminView.js`](file:///c:/Users/juand/Desktop/Tickets/Frontend/src/views/DashboardAdminView.js) consume el endpoint `GET /api/tickets/dashboard-admin` y renderiza tres gráficas interactivas mediante `vue-chartjs`:
1. **DoughnutChart**: Proporción del estado de tickets (Abiertos, En Progreso, Resueltos).
2. **BarChart (Vertical)**: Comparativa de fallas reportadas por Edificio / Ubicación.
3. **BarChart (Horizontal)**: Top de fallas por Categoría de servicio TI.

### 3.3. Motor Unificado de Filtros Dinámicos y Exportación Excel
Permite seleccionar de forma simultánea:
- **Rango de Fechas** (`fechaInicio`, `fechaFin`).
- **Edificio / Ubicación** (`ubicacionId`).
- **Categoría** (`categoriaId`).
- **Estado** (`estado`).
- **Prioridad** (`prioridad`).
- **Texto libre** (`busqueda`).

Los datos filtrados alimentan simultáneamente las gráficas, la tabla paginada y la descarga de reportes en Excel/CSV (codificado en UTF-8 BOM y delimitado de forma limpia).

### 3.4. Bitácora Timeline y Notificaciones Flotantes (Toasts)
- **Línea de Tiempo**: El componente de detalle [`DetalleTicketView.js`](file:///c:/Users/juand/Desktop/Tickets/Frontend/src/views/DetalleTicketView.js) despliega un *timeline* vertical que registra cronológicamente las observaciones, cambios de estado, asignaciones y notas.
- **ToastContainer**: Componente reactivo global que brinda retroalimentación al usuario tras cada operación de forma elegante.
- **Ficha PDF / Impresión**: Permite imprimir la ficha técnica oficial del ticket lista para firma de conformidad.

---

## 4. GUÍA DE MANTENIMIENTO Y EXTENSIÓN PARA DESARROLLADORES DE IA
Para agregar una nueva funcionalidad al proyecto:
1. **Crear Entidad o Campo**: Editar la clase correspondiente en `Backend/API/Domain/Entities/`.
2. **Agregar DTO / ViewModel**: Definir las validaciones `[Required]` o `[StringLength]` en `Backend/API/Application/ViewModels/`.
3. **Exponer Endpoint en Controlador**: Modificar o crear el controlador en `Backend/API/Controllers/`.
4. **Crear Servicio en Frontend**: Añadir la llamada `fetchApi` en `Frontend/src/services/`.
5. **Crear Vista / Componente Vue 3**: Crear la vista en `Frontend/src/views/` e integrarla al router declarativo de `App.js`.

---

## 5. CONCLUSIONES
La implementación exitosa del **Sistema de Tickets de Soporte TI TecNM Monclova** demuestra la viabilidad y eficiencia de desacoplar servicios con **.NET 10 y Vue 3**. La solución redujo los tiempos de atención, eliminó la duplicidad en los reportes y proporcionó a la dirección de la institución un panel analítico confiable con capacidad de exportación e impresión en tiempo real.
