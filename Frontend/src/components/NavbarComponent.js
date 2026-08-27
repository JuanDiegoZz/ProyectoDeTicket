export default {
  name: 'NavbarComponent',
  props: {
    usuario: Object,
    vistaActual: String
  },
  emits: ['navigate', 'logout'],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark navbar-tecnm sticky-top">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center gap-2" @click="$emit('navigate', 'home')" style="cursor: pointer;">
          <div class="bg-white p-1 rounded-2 d-flex align-items-center">
            <img src="/Backend/API/wwwroot/img/logo-limpio.png" alt="TecNM Logo" height="36" onerror="this.style.display='none'">
            <span class="fw-bold text-dark fs-6 px-1">TecNM</span>
          </div>
          <div class="d-flex flex-column">
            <span class="fw-bold fs-6 leading-tight">Soporte TI</span>
            <span class="small text-white-50" style="font-size: 0.7rem;">Campus Monclova</span>
          </div>
        </a>

        <button class="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#navbarTecnm">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarTecnm">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0 ms-lg-3" v-if="usuario">
            <li class="nav-item">
              <a class="nav-link nav-link-custom" :class="{ active: vistaActual.includes('dashboard') || vistaActual.includes('tickets') }" @click="$emit('navigate', 'home')">
                <i class="bi bi-speedometer2 me-1"></i>Panel Principal
              </a>
            </li>
            <li class="nav-item" v-if="usuario.rol === 'Solicitante'">
              <a class="nav-link nav-link-custom" :class="{ active: vistaActual === 'crear-ticket' }" @click="$emit('navigate', 'crear-ticket')">
                <i class="bi bi-plus-circle-fill me-1 text-warning"></i>Nuevo Reporte
              </a>
            </li>
            <template v-if="usuario.rol === 'Administrador'">
              <li class="nav-item">
                <a class="nav-link nav-link-custom" :class="{ active: vistaActual === 'gestion-tecnicos' }" @click="$emit('navigate', 'gestion-tecnicos')">
                  <i class="bi bi-person-badge-fill me-1 text-warning"></i>Técnicos
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link nav-link-custom" :class="{ active: vistaActual === 'categorias' }" @click="$emit('navigate', 'categorias')">
                  <i class="bi bi-tags-fill me-1 text-warning"></i>Categorías
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link nav-link-custom" :class="{ active: vistaActual === 'ubicaciones' }" @click="$emit('navigate', 'ubicaciones')">
                  <i class="bi bi-geo-alt-fill me-1 text-warning"></i>Ubicaciones
                </a>
              </li>
            </template>
          </ul>

          <div class="d-flex align-items-center gap-3 ms-auto">
            <template v-if="usuario">
              <div class="dropdown">
                <button class="btn btn-link text-white text-decoration-none dropdown-toggle d-flex align-items-center gap-2 p-0" type="button" data-bs-toggle="dropdown">
                  <div class="avatar-circle bg-warning text-dark fw-bold rounded-circle d-flex align-items-center justify-content-center" style="width:34px; height:34px;">
                    {{ usuario.nombreCompleto.charAt(0) }}
                  </div>
                  <div class="d-none d-md-flex flex-column text-start">
                    <span class="fw-semibold small leading-tight">{{ usuario.nombreCompleto }}</span>
                    <span class="badge bg-light text-dark text-uppercase" style="font-size:0.65rem;">{{ usuario.rol }}</span>
                  </div>
                </button>
                <ul class="dropdown-menu dropdown-menu-end shadow-sm">
                  <li><h6 class="dropdown-header">{{ usuario.email }}</h6></li>
                  <li><hr class="dropdown-divider"></li>
                  <li><a class="dropdown-item text-danger" @click="$emit('logout')"><i class="bi bi-box-arrow-right me-2"></i>Cerrar Sesión</a></li>
                </ul>
              </div>
            </template>
            <template v-else>
              <button class="btn btn-outline-light btn-sm rounded-pill px-3" @click="$emit('navigate', 'login')">Iniciar Sesión</button>
              <button class="btn btn-tecnm-gold btn-sm rounded-pill px-3" @click="$emit('navigate', 'registro')">Registrarse</button>
            </template>
          </div>
        </div>
      </div>
    </nav>
  `
};
