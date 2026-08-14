# 🎓 Sistema de Tickets de Soporte TI - TecNM Campus Monclova

Sistema web integral de mesa de ayuda y gestión de incidencias técnicas desarrollado para el Instituto Tecnológico Superior de Monclova.

---

## 🛠️ Tecnologías y Arquitectura
- **Backend:** C# (.NET 8) con ASP.NET Core MVC.
- **Base de Datos:** PostgreSQL 16 con Entity Framework Core (Code-First) y soporte dinámico local.
- **Seguridad y Cifrado:** BCrypt para hashing seguro de contraseñas y Cookie Authentication.
- **Frontend:** Vistas Razor responsivas con Bootstrap 5, Bootstrap Icons y hojas de estilo institucionales personalizadas.
- **Contenedores y Despliegue:** Docker, Docker Compose en Linux Debian (12/13).
- **Seguridad en Servidor:** Configuración UFW (Uncomplicated Firewall) + Fail2ban + aislamiento de red bridge en contenedores.

---

## 🚀 Funcionalidades y Cumplimiento de Requisitos

### 1. Control de Roles y Acceso
- **Administrador (Jefe de TI):**
  - Dashboard analítico con métricas en tiempo real (KPIs, fallas por edificio, eficiencia del personal técnico).
  - Exportación de auditoría e incidencias a **Excel (CSV UTF-8)**.
  - Buscador instantáneo y filtros combinados (*Abiertos, En Progreso, Resueltos*, prioridades).
  - **Módulo de reasignación de técnicos** con registro de motivo en bitácora.
  - **Gestión de Personal:** Alta, baja lógica y reactivación de técnicos.
- **Técnico de Soporte:**
  - Cola de atención técnica ágil (tickets asignados y tickets libres en campus).
  - Cambio de estados de incidencias (*Abierto &rarr; En Progreso &rarr; Resuelto*).
  - Bitácora de seguimiento para dialogar con los usuarios dentro del ticket.
- **Solicitante (Alumnos y Docentes):**
  - Registro institucional simplificado con **detección automática por Regex**:
    - **Alumnos** (`[Letra][8 dígitos]@monclova.tecnm.mx`) &rarr; **Prioridad Normal**.
    - **Docentes** (`nombre.apellido@monclova.tecnm.mx`) &rarr; **Prioridad Alta Automática**.
  - Subida de **evidencia fotográfica o capturas** (`.jpg`, `.png`, `.pdf`) con almacenamiento seguro en servidor.
  - **Encuesta de satisfacción con estrellas (1 a 5)** y comentarios una vez resuelto el ticket.

---

## 📦 Puesta en Marcha Rápida

### En Servidor Linux (Debian 12/13) con Docker Compose
```bash
# 1. Clonar o copiar los archivos al servidor
cd /opt/tickets-tecnm

# 2. Levantar la aplicación y base de datos PostgreSQL
docker compose up -d --build

# 3. Acceder en el navegador
http://<IP_DEL_SERVIDOR>:8080
```

### En Entorno de Desarrollo Local (Windows 11)
```powershell
dotnet run --launch-profile "http"
# Acceso: http://localhost:5000
```

---

## 🔑 Cuentas Institucionales de Prueba

| Rol | Correo Institucional | Contraseña |
| :--- | :--- | :--- |
| **Administrador TI** | `admin@monclova.tecnm.mx` | `Admin123!` |
| **Técnico de Soporte** | `carlos.tecnico@monclova.tecnm.mx` | `Tecnico123!` |
| **Docente (Prioridad Alta)** | `ruben.rr@monclova.tecnm.mx` | `Docente123!` |
| **Alumno (Prioridad Normal)** | `I22050319@monclova.tecnm.mx` | `Alumno123!` |

---

## 📄 Guía de Seguridad y Firewall
Para los comandos paso a paso de configuración de **UFW**, **Fail2ban** y políticas de puertos en Debian, consulta el archivo:
👉 [`DEPLOY_LINUX_DEBIAN.md`](DEPLOY_LINUX_DEBIAN.md)
