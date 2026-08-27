import { catalogoService } from '../services/catalogoService.js';

export default {
  name: 'CategoriasView',
  emits: ['navigate'],
  data() {
    return {
      categorias: [],
      nuevaNombre: '',
      nuevaDescripcion: '',
      editarId: null,
      editarNombre: '',
      editarDescripcion: '',
      editarActivo: true,
      cargando: true,
      error: null
    };
  },
  async created() {
    await this.cargarCategorias();
  },
  methods: {
    async cargarCategorias() {
      this.cargando = true;
      try {
        this.categorias = await catalogoService.obtenerCategorias(false);
      } catch (e) {
        this.error = e.message;
      } finally {
        this.cargando = false;
      }
    },
    async crearCategoria() {
      if (!this.nuevaNombre.trim()) return;
      try {
        await catalogoService.crearCategoria({ nombre: this.nuevaNombre, descripcion: this.nuevaDescripcion });
        this.nuevaNombre = '';
        this.nuevaDescripcion = '';
        await this.cargarCategorias();
      } catch (e) {
        this.error = e.message;
      }
    },
    abrirModalEditar(cat) {
      this.editarId = cat.id;
      this.editarNombre = cat.nombre;
      this.editarDescripcion = cat.descripcion;
      this.editarActivo = cat.activo;
    },
    async guardarEdicion() {
      try {
        await catalogoService.editarCategoria(this.editarId, {
          nombre: this.editarNombre,
          descripcion: this.editarDescripcion,
          activo: this.editarActivo
        });
        this.editarId = null;
        await this.cargarCategorias();
      } catch (e) {
        this.error = e.message;
      }
    },
    async alternarEstado(id) {
      try {
        await catalogoService.alternarEstadoCategoria(id);
        await this.cargarCategorias();
      } catch (e) {
        this.error = e.message;
      }
    }
  },
  template: `
    <div>
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 class="fw-bold mb-0 text-dark">Catálogo de Categorías (Vue 3)</h3>
          <p class="text-muted small mb-0">Gestión dinámicas de tipos de fallas técnicas</p>
        </div>
        <div>
          <button @click="$emit('navigate', 'ubicaciones')" class="btn btn-outline-primary rounded-3 me-2"><i class="bi bi-geo-alt-fill me-1"></i>Ubicaciones</button>
          <button @click="$emit('navigate', 'home')" class="btn btn-outline-secondary rounded-3"><i class="bi bi-arrow-left me-1"></i>Volver</button>
        </div>
      </div>

      <div v-if="error" class="alert alert-danger">{{ error }}</div>

      <!-- Crear Nueva Categoría Form -->
      <div class="card card-tecnm border-0 shadow-sm p-4 mb-4">
        <h6 class="fw-bold text-dark mb-3"><i class="bi bi-plus-circle me-2 text-primary"></i>Nueva Categoría de Soporte</h6>
        <form @submit.prevent="crearCategoria" class="row g-3">
          <div class="col-md-5">
            <input type="text" v-model="nuevaNombre" class="form-control form-control-custom" placeholder="Nombre de categoría (Ej. Impresoras)" required />
          </div>
          <div class="col-md-5">
            <input type="text" v-model="nuevaDescripcion" class="form-control form-control-custom" placeholder="Descripción breve..." />
          </div>
          <div class="col-md-2">
            <button type="submit" class="btn btn-tecnm-primary w-100"><i class="bi bi-check-lg me-1"></i>Guardar</button>
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
                  <th>Categoría</th>
                  <th>Descripción</th>
                  <th class="text-center">Estado</th>
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="cat in categorias" :key="cat.id">
                  <td class="fw-bold text-primary">#{{ cat.id }}</td>
                  <td class="fw-bold text-dark">{{ cat.nombre }}</td>
                  <td class="text-muted small">{{ cat.descripcion || 'Sin descripción' }}</td>
                  <td class="text-center">
                    <span v-if="cat.activo" class="badge-status-resuelto">Activa</span>
                    <span v-else class="badge bg-secondary text-white rounded-pill px-3 py-1">Inactiva</span>
                  </td>
                  <td class="text-end">
                    <button @click="abrirModalEditar(cat)" class="btn btn-sm btn-outline-primary rounded-2 me-1" data-bs-toggle="modal" data-bs-target="#modalEditarCat"><i class="bi bi-pencil me-1"></i>Editar</button>
                    <button @click="alternarEstado(cat.id)" class="btn btn-sm" :class="cat.activo ? 'btn-outline-danger' : 'btn-outline-success'">
                      {{ cat.activo ? 'Desactivar' : 'Activar' }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Modal Editar Categoría -->
      <div class="modal fade" id="modalEditarCat" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 shadow-lg rounded-4">
            <div class="modal-header bg-tecnm-blue text-white">
              <h6 class="modal-title fw-bold">Editar Categoría #{{ editarId }}</h6>
              <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body p-4">
              <div class="mb-3">
                <label class="form-label small fw-semibold text-secondary">Nombre</label>
                <input type="text" v-model="editarNombre" class="form-control form-control-custom" required />
              </div>
              <div class="mb-3">
                <label class="form-label small fw-semibold text-secondary">Descripción</label>
                <textarea v-model="editarDescripcion" class="form-control form-control-custom" rows="3"></textarea>
              </div>
              <div class="form-check form-switch">
                <input class="form-check-input" type="checkbox" v-model="editarActivo" id="checkCatActivo" />
                <label class="form-check-label fw-semibold" for="checkCatActivo">Habilitada</label>
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
