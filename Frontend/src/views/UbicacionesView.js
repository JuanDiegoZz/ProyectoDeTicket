import { catalogoService } from '../services/catalogoService.js';

export default {
  name: 'UbicacionesView',
  emits: ['navigate'],
  data() {
    return {
      ubicaciones: [],
      nuevaNombre: '',
      editarId: null,
      editarNombre: '',
      editarActivo: true,
      cargando: true,
      error: null
    };
  },
  async created() {
    await this.cargarUbicaciones();
  },
  methods: {
    async cargarUbicaciones() {
      this.cargando = true;
      try {
        this.ubicaciones = await catalogoService.obtenerUbicaciones(false);
      } catch (e) {
        this.error = e.message;
      } finally {
        this.cargando = false;
      }
    },
    async crearUbicacion() {
      if (!this.nuevaNombre.trim()) return;
      try {
        await catalogoService.crearUbicacion({ nombre: this.nuevaNombre });
        this.nuevaNombre = '';
        await this.cargarUbicaciones();
      } catch (e) {
        this.error = e.message;
      }
    },
    abrirModalEditar(ub) {
      this.editarId = ub.id;
      this.editarNombre = ub.nombre;
      this.editarActivo = ub.activo;
    },
    async guardarEdicion() {
      try {
        await catalogoService.editarUbicacion(this.editarId, {
          nombre: this.editarNombre,
          activo: this.editarActivo
        });
        this.editarId = null;
        await this.cargarUbicaciones();
      } catch (e) {
        this.error = e.message;
      }
    },
    async alternarEstado(id) {
      try {
        await catalogoService.alternarEstadoUbicacion(id);
        await this.cargarUbicaciones();
      } catch (e) {
        this.error = e.message;
      }
    }
  },
  template: `
    <div>
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 class="fw-bold mb-0 text-dark">Catálogo de Ubicaciones (Vue 3)</h3>
          <p class="text-muted small mb-0">Gestión de edificios e instalaciones del campus</p>
        </div>
        <div>
          <button @click="$emit('navigate', 'categorias')" class="btn btn-outline-primary rounded-3 me-2"><i class="bi bi-tags-fill me-1"></i>Categorías</button>
          <button @click="$emit('navigate', 'home')" class="btn btn-outline-secondary rounded-3"><i class="bi bi-arrow-left me-1"></i>Volver</button>
        </div>
      </div>

      <div v-if="error" class="alert alert-danger">{{ error }}</div>

      <!-- Crear Nueva Ubicación Form -->
      <div class="card card-tecnm border-0 shadow-sm p-4 mb-4">
        <h6 class="fw-bold text-dark mb-3"><i class="bi bi-plus-circle me-2 text-primary"></i>Nueva Ubicación del Campus</h6>
        <form @submit.prevent="crearUbicacion" class="row g-3">
          <div class="col-md-9">
            <input type="text" v-model="nuevaNombre" class="form-control form-control-custom" placeholder="Nombre de edificio (Ej. Biblioteca Central / Edificio D)" required />
          </div>
          <div class="col-md-3">
            <button type="submit" class="btn btn-tecnm-primary w-100"><i class="bi bi-check-lg me-1"></i>Guardar Ubicación</button>
          </div>
        </form>
      </div>

      <div class="card card-tecnm border-0 shadow-sm overflow-hidden">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-tecnm table-hover mb-0">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Edificio / Área</th>
                  <th class="text-center">Estado</th>
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="ub in ubicaciones" :key="ub.id">
                  <td class="fw-bold text-primary">#{{ ub.id }}</td>
                  <td class="fw-bold text-dark"><i class="bi bi-building me-2 text-primary"></i>{{ ub.nombre }}</td>
                  <td class="text-center">
                    <span v-if="ub.activo" class="badge-status-resuelto">Habilitada</span>
                    <span v-else class="badge bg-secondary text-white rounded-pill px-3 py-1">Inactiva</span>
                  </td>
                  <td class="text-end">
                    <button @click="abrirModalEditar(ub)" class="btn btn-sm btn-outline-primary rounded-2 me-1" data-bs-toggle="modal" data-bs-target="#modalEditarUb"><i class="bi bi-pencil me-1"></i>Editar</button>
                    <button @click="alternarEstado(ub.id)" class="btn btn-sm" :class="ub.activo ? 'btn-outline-danger' : 'btn-outline-success'">
                      {{ ub.activo ? 'Desactivar' : 'Activar' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Modal Editar Ubicación -->
      <div class="modal fade" id="modalEditarUb" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 shadow-lg rounded-4">
            <div class="modal-header bg-tecnm-blue text-white">
              <h6 class="modal-title fw-bold">Editar Ubicación #{{ editarId }}</h6>
              <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body p-4">
              <div class="mb-3">
                <label class="form-label small fw-semibold text-secondary">Edificio / Instalación</label>
                <input type="text" v-model="editarNombre" class="form-control form-control-custom" required />
              </div>
              <div class="form-check form-switch">
                <input class="form-check-input" type="checkbox" v-model="editarActivo" id="checkUbActivo" />
                <label class="form-check-label fw-semibold" for="checkUbActivo">Habilitada</label>
              </div>
            </div>
            <div class="modal-footer bg-light">
              <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal">Cancelar</button>
              <button type="button" @click="guardarEdicion" class="btn btn-tecnm-primary btn-sm" data-bs-dismiss="modal">Guardar Cambios</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
};
