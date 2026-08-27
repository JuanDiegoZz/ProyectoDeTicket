import { authService } from '../services/authService.js';

export default {
  name: 'RegistroView',
  emits: ['navigate'],
  data() {
    return {
      nombreCompleto: '',
      email: '',
      password: '',
      confirmPassword: '',
      error: null,
      exito: null,
      cargando: false
    };
  },
  methods: {
    async handleSubmit() {
      this.error = null;
      this.exito = null;

      if (this.password !== this.confirmPassword) {
        this.error = 'Las contraseñas no coinciden.';
        return;
      }

      this.cargando = true;
      try {
        await authService.registro(this.nombreCompleto, this.email, this.password, this.confirmPassword);
        this.exito = '¡Registro exitoso! Ya puedes iniciar sesión con tus credenciales.';
        setTimeout(() => this.$emit('navigate', 'login'), 2000);
      } catch (err) {
        this.error = err.message || 'Error al registrar usuario.';
      } finally {
        this.cargando = false;
      }
    }
  },
  template: `
    <div class="row justify-content-center my-auto py-5">
      <div class="col-12 col-md-7 col-lg-6">
        <div class="card card-tecnm border-0 shadow-lg overflow-hidden">
          <div class="card-body p-4 p-md-5">
            <div class="text-center mb-4">
              <div class="d-inline-block mb-3">
                <img src="/src/assets/img/logo-tecnm-vertical-small.png" alt="TecNM Logo" height="70" class="login-logo-img" />
              </div>
              <h4 class="fw-extrabold text-dark mb-1">Registro de Usuario</h4>
              <p class="text-muted small">Crea tu cuenta institucional (@monclova.tecnm.mx)</p>
            </div>

            <div v-if="error" class="alert alert-danger text-center small"><i class="bi bi-exclamation-triangle-fill me-2"></i>{{ error }}</div>
            <div v-if="exito" class="alert alert-success text-center small"><i class="bi bi-check-circle-fill me-2"></i>{{ exito }}</div>

            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Nombre Completo</label>
                <input type="text" v-model="nombreCompleto" class="form-control form-control-custom" placeholder="Ej. Juan Pérez Tolentino" required />
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Correo Institucional</label>
                <input type="email" v-model="email" class="form-control form-control-custom" placeholder="I22050319@monclova.tecnm.mx" required />
                <div class="form-text text-muted small">Alumnos y docentes reciben asignación automática de prioridad según dominio.</div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-md-6">
                  <label class="form-label fw-semibold small text-secondary text-uppercase">Contraseña</label>
                  <input type="password" v-model="password" class="form-control form-control-custom" placeholder="Mínimo 6 caracteres" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-semibold small text-secondary text-uppercase">Confirmar Contraseña</label>
                  <input type="password" v-model="confirmPassword" class="form-control form-control-custom" placeholder="Repite la contraseña" required />
                </div>
              </div>

              <button type="submit" class="btn btn-tecnm-gold w-100 py-2 fw-bold" :disabled="cargando">
                <span v-if="cargando" class="spinner-border spinner-border-sm me-2"></span>
                <span v-else><i class="bi bi-person-plus-fill me-2"></i>Crear Cuenta</span>
              </button>
            </form>

            <div class="text-center mt-4 pt-3 border-top">
              <span class="text-muted small">¿Ya tienes una cuenta?</span>
              <a class="fw-bold text-primary ms-2" @click="$emit('navigate', 'login')" style="cursor:pointer;">Inicia sesión</a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
};
