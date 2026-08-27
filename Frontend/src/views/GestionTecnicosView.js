import { tecnicoService } from '../services/tecnicoService.js';

export default {
  name: 'GestionTecnicosView',
  emits: ['navigate'],
  data() {
    return {
      tecnicos: [],
      nombreCompleto: '',
      email: '',
      password: '',
      confirmPassword: '',
      cargando: true,
      error: null,
      exito: null
    };
  },
  async created() {
    await this.cargarTecnicos();
  },
  methods: {
    async cargarTecnicos() {
      this.cargando = true;
      try {
        this.tecnicos = await tecnicoService.obtenerTecnicos();
      } catch (e) {
        this.error = e.message;
      } finally {
        this.cargando = false;
      }
    },
    async crearTecnico() {
      this.error = null;
      this.exito = null;

      if (this.password !== this.confirmPassword) {
        this.error = 'Las contraseñas no coinciden.';
        return;
      }

      try {
        await tecnicoService.crearTecnico(this.nombreCompleto, this.email, this.password, this.confirmPassword);
        this.exito = 'Técnico creado exitosamente.';
        this.nombreCompleto = '';
        this.email = '';
        this.password = '';
        this.confirmPassword = '';
        await this.cargarTecnicos();
      } catch (e) {
        this.error = e.message;
      }
    },
    async alternarEstado(id) {
      try {
        await tecnicoService.alternarEstadoTecnico(id);
        await this.cargarTecnicos();
      } catch (e) {
        this.error = e.message;
      }
    }
  },
  template: `
    <div>
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 class="fw-bold mb-0 text-dark">Gestión de Personal Técnico (Vue 3)</h3>
          <p class="text-muted small mb-0">Alta, baja y control de acceso del equipo de soporte</p>
        </div>
        <button @click="$emit('navigate', 'home')" class="btn btn-outline-secondary rounded-3"><i class="bi bi-arrow-left me-1"></i>Volver</button>
      </div>

      <div v-if="error" class="alert alert-danger">{{ error }}</div>
      <div v-if="exito" class="alert alert-success">{{ exito }}</div>

      <!-- Formulario Nuevo Técnico -->
      <div class="card card-tecnm border-0 shadow-sm p-4 mb-4">
        <h6 class="fw-bold text-dark mb-3"><i class="bi bi-person-plus-fill me-2 text-primary"></i>Registrar Nuevo Técnico</h6>
        <form @submit.prevent="crearTecnico" class="row g-3">
          <div class="col-md-3">
            <input type="text" v-model="nombreCompleto" class="form-control form-control-custom" placeholder="Nombre completo" required />
          </div>
          <div class="col-md-3">
            <input type="email" v-model="email" class="form-control form-control-custom" placeholder="correo@monclova.tecnm.mx" required />
          </div>
          <div class="col-md-2">
            <input type="password" v-model="password" class="form-control form-control-custom" placeholder="Contraseña" required />
          </div>
          <div class="col-md-2">
            <input type="password" v-model="confirmPassword" class="form-control form-control-custom" placeholder="Confirmar" required />
          </div>
          <div class="col-md-2">
            <button type="submit" class="btn btn-tecnm-primary w-100"><i class="bi bi-plus-lg me-1"></i>Alta Técnico</button>
          </div>
        </form>
      </div>

      <div class="card card-tecnm border-0 shadow-sm overflow-hidden">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-tecnm table-hover mb-0">
              <thead>
                <tr>
                  <th>Técnico</th>
                  <th>Correo Institucional</th>
                  <th>Fecha de Registro</th>
                  <th class="text-center">Estado</th>
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="t in tecnicos" :key="t.id">
                  <td class="fw-bold text-dark"><i class="bi bi-person-badge me-2 text-primary"></i>{{ t.nombreCompleto }}</td>
                  <td><code>{{ t.email }}</code></td>
                  <td class="small text-muted">{{ new Date(t.fechaRegistro).toLocaleDateString() }}</td>
                  <td class="text-center">
                    <span v-if="t.activo" class="badge-status-resuelto">Activo</span>
                    <span v-else class="badge bg-secondary text-white rounded-pill px-3 py-1">Inactivo</span>
                  </td>
                  <td class="text-end">
                    <button @click="alternarEstado(t.id)" class="btn btn-sm" :class="t.activo ? 'btn-outline-danger' : 'btn-outline-success'">
                      {{ t.activo ? 'Baja Lógica' : 'Reactivar' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `
};
