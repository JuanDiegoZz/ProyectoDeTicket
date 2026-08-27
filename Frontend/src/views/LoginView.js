import { authService } from '../services/authService.js';

export default {
  name: 'LoginView',
  emits: ['login-success', 'navigate'],
  data() {
    return {
      email: '',
      password: '',
      error: null,
      cargando: false
    };
  },
  methods: {
    async handleSubmit() {
      this.error = null;
      this.cargando = true;
      try {
        const usuario = await authService.login(this.email, this.password);
        this.$emit('login-success', usuario);
      } catch (err) {
        this.error = err.message || 'Error de autenticación.';
      } finally {
        this.cargando = false;
      }
    }
  },
  template: `
    <div class="row justify-content-center my-auto py-5">
      <div class="col-12 col-md-6 col-lg-5">
        <div class="card card-tecnm border-0 shadow-lg overflow-hidden">
          <div class="card-body p-4 p-md-5">
            <div class="text-center mb-4">
              <div class="bg-white p-2 rounded-3 d-inline-block shadow-sm mb-3">
                <span class="fs-1 text-primary"><i class="bi bi-shield-lock-fill"></i></span>
              </div>
              <h4 class="fw-extrabold text-dark mb-1">Iniciar Sesión</h4>
              <p class="text-muted small">Sistema de Tickets de Soporte TI — TecNM Monclova</p>
            </div>

            <div v-if="error" class="alert alert-danger alert-dismissible fade show text-center small" role="alert">
              <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ error }}
            </div>

            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Correo Institucional</label>
                <div class="input-group">
                  <span class="input-group-text bg-light border-end-0"><i class="bi bi-envelope text-muted"></i></span>
                  <input type="email" v-model="email" class="form-control form-control-custom border-start-0" placeholder="usuario@monclova.tecnm.mx" required />
                </div>
              </div>

              <div class="mb-4">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Contraseña</label>
                <div class="input-group">
                  <span class="input-group-text bg-light border-end-0"><i class="bi bi-lock text-muted"></i></span>
                  <input type="password" v-model="password" class="form-control form-control-custom border-start-0" placeholder="••••••••" required />
                </div>
              </div>

              <button type="submit" class="btn btn-tecnm-primary w-100 py-2 fw-bold" :disabled="cargando">
                <span v-if="cargando" class="spinner-border spinner-border-sm me-2"></span>
                <span v-else><i class="bi bi-box-arrow-in-right me-2"></i>Ingresar al Sistema</span>
              </button>
            </form>

            <div class="text-center mt-4 pt-3 border-top">
              <span class="text-muted small">¿No tienes cuenta institucional?</span>
              <a class="fw-bold text-primary ms-2" @click="$emit('navigate', 'registro')" style="cursor:pointer;">Regístrate aquí</a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
};
