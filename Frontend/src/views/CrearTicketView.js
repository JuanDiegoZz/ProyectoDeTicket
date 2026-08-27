import { ticketService } from '../services/ticketService.js';
import { catalogoService } from '../services/catalogoService.js';

export default {
  name: 'CrearTicketView',
  emits: ['navigate'],
  data() {
    return {
      titulo: '',
      descripcion: '',
      categoriaId: '',
      ubicacionId: '',
      detalleAula: '',
      archivoEvidencia: null,
      categorias: [],
      ubicaciones: [],
      error: null,
      cargando: false
    };
  },
  async created() {
    try {
      this.categorias = await catalogoService.obtenerCategorias(true);
      this.ubicaciones = await catalogoService.obtenerUbicaciones(true);
    } catch (e) {
      this.error = 'No se pudieron cargar los catálogos.';
    }
  },
  methods: {
    handleFileUpload(event) {
      this.archivoEvidencia = event.target.files[0] || null;
    },
    async handleSubmit() {
      this.error = null;
      this.cargando = true;

      const formData = new FormData();
      formData.append('titulo', this.titulo);
      formData.append('descripcion', this.descripcion);
      formData.append('categoriaId', this.categoriaId);
      formData.append('ubicacionId', this.ubicacionId);
      if (this.detalleAula) formData.append('detalleAula', this.detalleAula);
      if (this.archivoEvidencia) formData.append('archivoEvidencia', this.archivoEvidencia);

      try {
        await ticketService.crearTicket(formData);
        this.$emit('navigate', 'home');
      } catch (err) {
        this.error = err.message || 'Error al generar el ticket.';
      } finally {
        this.cargando = false;
      }
    }
  },
  template: `
    <div class="row justify-content-center">
      <div class="col-12 col-lg-8">
        <div class="card card-tecnm border-0 shadow-lg overflow-hidden">
          <div class="card-header bg-tecnm-blue text-white p-4" style="background: var(--tecnm-accent-gradient);">
            <h4 class="fw-bold mb-0"><i class="bi bi-headset me-2 text-warning"></i>Nuevo Reporte de Incidencia TI</h4>
            <p class="small text-white-50 mb-0">Completa la información detallada de la falla para asignar el soporte técnico oportuno</p>
          </div>
          <div class="card-body p-4 p-md-5">
            <div v-if="error" class="alert alert-danger"><i class="bi bi-exclamation-triangle-fill me-2"></i>{{ error }}</div>

            <form @submit.prevent="handleSubmit">
              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Título o Asunto Principal</label>
                <input type="text" v-model="titulo" class="form-control form-control-custom" placeholder="Ej. Proyector sin señal HDMI en clase de Física" required />
              </div>

              <div class="row g-3 mb-3">
                <div class="col-md-6">
                  <label class="form-label fw-semibold small text-secondary text-uppercase">Categoría / Tipo de Falla</label>
                  <select v-model="categoriaId" class="form-select form-select-custom" required>
                    <option value="">-- Selecciona una Categoría --</option>
                    <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
                  </select>
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-semibold small text-secondary text-uppercase">Ubicación / Edificio</label>
                  <select v-model="ubicacionId" class="form-select form-select-custom" required>
                    <option value="">-- Selecciona un Edificio --</option>
                    <option v-for="u in ubicaciones" :key="u.id" :value="u.id">{{ u.nombre }}</option>
                  </select>
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Detalle de Aula / Oficina / Laboratorio</label>
                <input type="text" v-model="detalleAula" class="form-control form-control-custom" placeholder="Ej. Aula 104, Laboratorio de Redes..." />
              </div>

              <div class="mb-3">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Descripción Detallada del Problema</label>
                <textarea v-model="descripcion" class="form-control form-control-custom" rows="4" placeholder="Explica detalladamente qué ocurre, mensajes de error o comportamientos anómalos..." required></textarea>
              </div>

              <div class="mb-4">
                <label class="form-label fw-semibold small text-secondary text-uppercase">Evidencia en Imagen o PDF (Opcional)</label>
                <input type="file" @change="handleFileUpload" class="form-control form-control-custom" accept=".jpg,.jpeg,.png,.webp,.pdf" />
              </div>

              <div class="d-flex justify-content-end gap-2 pt-3 border-top">
                <button type="button" @click="$emit('navigate', 'home')" class="btn btn-outline-secondary rounded-pill px-4">Cancelar</button>
                <button type="submit" class="btn btn-tecnm-primary rounded-pill px-4" :disabled="cargando">
                  <span v-if="cargando" class="spinner-border spinner-border-sm me-2"></span>
                  <span v-else><i class="bi bi-send-fill me-2"></i>Enviar Reporte</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  `
};
