# 🎓 Sistema de Gestión de Tickets de Soporte TI — TecNM Campus Monclova

Sistema web integral de mesa de ayuda y gestión de incidencias técnicas desarrollado para el Instituto Tecnológico Superior de Monclova bajo una **arquitectura desacoplada de alto rendimiento**.

---

## 🛠️ Tecnologías y Arquitectura Desacoplada

- **Backend API REST:** C# **.NET 10 (SDK net10.0)** con ASP.NET Core Web API, Clean Architecture, DTOs y respuestas JSON en `camelCase`.
- **Frontend SPA:** Single Page Application construida en **Vue 3** (Composition API & Options API) con librerías nativas **`vue-chartjs` 5.3 + `Chart.js` 4.4**, Bootstrap 5.3, Bootstrap Icons y alertas con **SweetAlert2**.
- **Base de Datos:** **PostgreSQL 18** en producción (vía Docker) / SQLite en desarrollo local con Entity Framework Core 10 (Code-First) y tablas en `snake_case`.
- **Seguridad y Cifrado:** BCrypt.Net-Next para hashing de contraseñas, Cookie Authentication (`TecNMTicketsAuthCookie` con `SameSite=Lax`) y validaciones server-side.
- **Identidad Visual Oficial:** Paleta de colores oficial TecNM (Azul Pantone 294 C `#1B396A`, Cool Gray 10 C `#807E82`, `#000000`), tipografía secundaria institucional **Montserrat** y Modo Oscuro (*Dark Mode*).
- **Contenedores y Despliegue:** Docker, Docker Compose, Nginx Reverse Proxy en Linux Debian (12 / 13).

---

## 🚀 Funcionalidades Principales

### 1. Control de Roles y Acceso Diferenciado
- **Administrador (Jefe de TI):**
  - Dashboard analítico con gráficas interactivas (`DoughnutChart` de estados, `BarChart` de fallas por edificio y por categoría).
  - Indicadores **KPI de SLA** (Tiempo Promedio de Respuesta en Horas y Tasa de Eficiencia %).
  - Filtros avanzados colapsables por **Rango de Fechas**, **Edificio**, **Categoría**, **Estado**, **Prioridad** y **Búsqueda libre por texto**.
  - Exportación de auditoría e incidencias a **Excel / CSV Saneado (UTF-8 BOM)**.
  - Reasignación de técnicos con registro de motivo en bitácora y gestión de catálogos (Edificios, Categorías, Técnicos).
- **Técnico de Soporte:**
  - Cola de atención de tickets asignados.
  - Cambio de estados de incidencias (*Abierto &rarr; En Progreso &rarr; Resuelto*) y registro de notas de avance.
- **Solicitante (Alumnos y Docentes):**
  - Registro institucional con **detección automática por correo**:
    - **Docentes / Jefaturas** (`@monclova.tecnm.mx`) &rarr; **Prioridad Alta Automática**.
    - **Alumnos** &rarr; **Prioridad Normal Automática**.
  - Registro de tickets con evidencia adjunta y **línea de tiempo (*Timeline*)** de seguimiento.
  - Encuesta de satisfacción con estrellas (1 a 5) e **impresión de Ficha Técnica PDF**.

---

## 📦 Puesta en Marcha Rápida

### 1. Servidores Locales de Desarrollo

#### Backend API REST (.NET 10)
```powershell
cd Backend/API
dotnet run --urls "http://localhost:5000"
# API REST escuchando en: http://localhost:5000/api
```

#### Frontend Vue 3 SPA
```powershell
# Abrir un navegador o servidor de archivos estáticos en la carpeta Frontend
# Acceso en navegador: http://localhost:5173/
```

---

## 🔑 Credenciales de Prueba Sembradas

| Rol | Correo Institucional | Contraseña |
| :--- | :--- | :--- |
| **Administrador** | `admin@monclova.tecnm.mx` | `Admin123!` |
| **Técnico TI** | `carlos.tecnico@monclova.tecnm.mx` | `Tecnico123!` |
| **Solicitante** | `alumno@monclova.tecnm.mx` | `Usuario123!` |

---

## 📚 Documentación Técnica Detallada

- **[DOCUMENTACION_ARTICULO_CIENTIFICO.md](file:///c:/Users/juand/Desktop/Tickets/DOCUMENTACION_ARTICULO_CIENTIFICO.md)**: Artículo técnico y académico formal.
- **[DEPLOY_LINUX_DEBIAN.md](file:///c:/Users/juand/Desktop/Tickets/DEPLOY_LINUX_DEBIAN.md)**: Manual de despliegue en servidores Linux Debian 12/13 (Docker + Nginx + PostgreSQL 18).
- **[DESIGN.md](file:///c:/Users/juand/Desktop/Tickets/DESIGN.md)**: Documento de diseño arquitectónico y de datos.
- **[Backend/README.md](file:///c:/Users/juand/Desktop/Tickets/Backend/README.md)**: Documentación del módulo API REST .NET 10.
- **[Frontend/README.md](file:///c:/Users/juand/Desktop/Tickets/Frontend/README.md)**: Documentación del módulo Vue 3 SPA.
