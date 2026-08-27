import { authService } from './services/authService.js';
import LoginView from './views/LoginView.js';
import RegistroView from './views/RegistroView.js';
import DashboardAdminView from './views/DashboardAdminView.js';
import TicketsTecnicoView from './views/TicketsTecnicoView.js';
import TicketsSolicitanteView from './views/TicketsSolicitanteView.js';
import DetalleTicketView from './views/DetalleTicketView.js';
import CrearTicketView from './views/CrearTicketView.js';
import CategoriasView from './views/CategoriasView.js';
import UbicacionesView from './views/UbicacionesView.js';
import GestionTecnicosView from './views/GestionTecnicosView.js';
import NavbarComponent from './components/NavbarComponent.js';

export default {
  name: 'App',
  components: {
    NavbarComponent,
    LoginView,
    RegistroView,
    DashboardAdminView,
    TicketsTecnicoView,
    TicketsSolicitanteView,
    DetalleTicketView,
    CrearTicketView,
    CategoriasView,
    UbicacionesView,
    GestionTecnicosView
  },
  data() {
    return {
      usuarioActual: null,
      vistaActual: 'login',
      ticketSeleccionadoId: null,
      cargando: true
    };
  },
  async created() {
    window.addEventListener('auth-unauthorized', () => {
      this.usuarioActual = null;
      this.vistaActual = 'login';
    });

    await this.verificarSesion();
  },
  methods: {
    async verificarSesion() {
      this.cargando = true;
      try {
        const user = await authService.obtenerUsuarioActual();
        this.usuarioActual = user;
        this.navegarA('home');
      } catch (e) {
        this.usuarioActual = null;
        if (this.vistaActual !== 'registro') {
          this.vistaActual = 'login';
        }
      } finally {
        this.cargando = false;
      }
    },
    navegarA(vista, paramId = null) {
      if (paramId) this.ticketSeleccionadoId = paramId;

      if (!this.usuarioActual && vista !== 'login' && vista !== 'registro') {
        this.vistaActual = 'login';
        return;
      }

      if (vista === 'home') {
        if (this.usuarioActual.rol === 'Administrador') this.vistaActual = 'dashboard-admin';
        else if (this.usuarioActual.rol === 'Tecnico') this.vistaActual = 'tickets-tecnico';
        else this.vistaActual = 'tickets-solicitante';
        return;
      }

      this.vistaActual = vista;
    },
    alIniciarSesion(usuario) {
      this.usuarioActual = usuario;
      this.navegarA('home');
    },
    async alCerrarSesion() {
      try {
        await authService.logout();
      } catch (e) {}
      this.usuarioActual = null;
      this.vistaActual = 'login';
    }
  },
  template: `
    <div class="d-flex flex-column min-vh-100">
      <navbar-component 
        :usuario="usuarioActual" 
        :vista-actual="vistaActual"
        @navigate="navegarA"
        @logout="alCerrarSesion"
      />

      <main class="flex-grow-1 container py-4">
        <div v-if="cargando" class="text-center py-5">
          <div class="spinner-border text-primary" role="status"></div>
          <p class="text-muted mt-2">Cargando Sistema de Tickets TecNM...</p>
        </div>

        <template v-else>
          <login-view 
            v-if="vistaActual === 'login'" 
            @login-success="alIniciarSesion"
            @navigate="navegarA"
          />

          <registro-view 
            v-else-if="vistaActual === 'registro'" 
            @navigate="navegarA"
          />

          <dashboard-admin-view 
            v-else-if="vistaActual === 'dashboard-admin'" 
            @navigate="navegarA"
          />

          <tickets-tecnico-view 
            v-else-if="vistaActual === 'tickets-tecnico'" 
            @navigate="navegarA"
          />

          <tickets-solicitante-view 
            v-else-if="vistaActual === 'tickets-solicitante'" 
            @navigate="navegarA"
          />

          <detalle-ticket-view 
            v-else-if="vistaActual === 'detalle-ticket'" 
            :ticket-id="ticketSeleccionadoId"
            :usuario-actual="usuarioActual"
            @navigate="navegarA"
          />

          <crear-ticket-view 
            v-else-if="vistaActual === 'crear-ticket'" 
            @navigate="navegarA"
          />

          <categorias-view 
            v-else-if="vistaActual === 'categorias'" 
            @navigate="navegarA"
          />

          <ubicaciones-view 
            v-else-if="vistaActual === 'ubicaciones'" 
            @navigate="navegarA"
          />

          <gestion-tecnicos-view 
            v-else-if="vistaActual === 'gestion-tecnicos'" 
            @navigate="navegarA"
          />
        </template>
      </main>

      <footer class="bg-white border-top py-3 text-center text-muted small mt-auto">
        <div class="container">
          <span>&copy; 2026 Instituto Tecnológico Superior de Monclova — Sistema de Gestión de Tickets de Soporte TI</span>
        </div>
      </footer>
    </div>
  `
};
