# Frontend - Vue 3 SPA (TecNM Soporte TI)

## 1. Propósito del Módulo
El Frontend es una aplicación **Single Page Application (SPA)** desarrollada en **Vue 3** que se encarga exclusivamente de la interfaz de usuario, navegación reactiva, formularios, gráficas analíticas y consumo de la API REST del Backend mediante HTTP / JSON.

---

## 2. Estructura de Directorios
```
Frontend/
├── index.html
├── src/
│   ├── assets/
│   │   └── styles.css (Tema Claro y Modo Oscuro)
│   ├── services/
│   │   ├── apiConfig.js (Configuración centralizada de Fetch API)
│   │   ├── authService.js
│   │   ├── catalogoService.js
│   │   ├── ticketService.js
│   │   └── tecnicoService.js
│   ├── components/
│   │   └── NavbarComponent.js
│   ├── views/
│   │   ├── LoginView.js
│   │   ├── RegistroView.js
│   │   ├── DashboardAdminView.js
│   │   ├── TicketsTecnicoView.js
│   │   ├── TicketsSolicitanteView.js
│   │   ├── DetalleTicketView.js
│   │   ├── CrearTicketView.js
│   │   ├── CategoriasView.js
│   │   ├── UbicacionesView.js
│   │   └── GestionTecnicosView.js
│   ├── App.js
│   └── main.js
└── README.md
```

---

## 3. Configuración y Conexión con la API REST
Toda solicitud HTTP está centralizada en `src/services/apiConfig.js`:
- **`API_BASE_URL`**: `http://localhost:5000/api`
- **Manejo de credenciales**: `credentials: 'include'` para envío automático de cookies de sesión.
- **Manejo de errores**: Redirección reactiva ante errores `401 Unauthorized`.

---

## 4. Ejecución del Frontend
El Frontend Vue 3 funciona como SPA servida directamente desde cualquier servidor HTTP estático (Vite, Nginx, Python http.server, etc.) o se puede abrir directamente en el navegador apuntando a la API REST.

```bash
# Ejemplo de servidor local con Python en carpeta Frontend:
python -m http.server 5173
```
Acceder en navegador a: `http://localhost:5173`
