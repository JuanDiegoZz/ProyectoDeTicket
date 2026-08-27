import { ticketService } from '../services/ticketService.js';
import { catalogoService } from '../services/catalogoService.js';
import DoughnutChart from '../components/charts/DoughnutChart.js';
import BarChart from '../components/charts/BarChart.js';

export default {
  name: 'DashboardAdminView',
  components: {
    DoughnutChart,
    BarChart
  },
  emits: ['navigate'],
  data() {
    return {
      metrics: null,
      pagedResult: { items: [], totalItems: 0, pageNumber: 1, totalPages: 1 },
      categorias: [],
      ubicaciones: [],
      filtros: {
        busqueda: '',
        estado: '',
        prioridad: '',
        categoriaId: '',
        ubicacionId: '',
        orden: '',
        pagina: 1
      },
      cargando: true
    };
  },
  computed: {
    chartEstadosData() {
      if (!this.metrics) return null;
      return {
        labels: ['Abiertos', 'En Progreso', 'Resueltos'],
        datasets: [{
          data: [this.metrics.ticketsAbiertos, this.metrics.ticketsEnProgreso, this.metrics.ticketsResueltos],
          backgroundColor: ['#F59E0B', '#0284C7', '#10B981'],
          borderWidth: 2,
          borderColor: '#ffffff'
        }]
      };
    },
    chartUbicacionesData() {
      if (!this.metrics || !this.metrics.fallasPorUbicacion) return null;
      return {
        labels: this.metrics.fallasPorUbicacion.map(x => x.ubicacion),
        datasets: [{
          label: 'Tickets Reportados',
          data: this.metrics.fallasPorUbicacion.map(x => x.cantidad),
          backgroundColor: '#1B396A',
          borderRadius: 6
        }]
      };
    },
    chartCategoriasData() {
      if (!this.metrics || !this.metrics.fallasPorCategoria) return null;
      return {
        labels: this.metrics.fallasPorCategoria.map(x => x.categoria),
        datasets: [{
          label: 'Tickets',
          data: this.metrics.fallasPorCategoria.map(x => x.cantidad),
          backgroundColor: '#D4AF37',
          borderRadius: 6
        }]
      };
    },
    chartCategoriasOptions() {
      return {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } }
      };
    }
  },
  async created() {
    await this.cargarCatalogos();
    await this.cargarDatos();
  },
  methods: {
    async cargarCatalogos() {
      try {
        this.categorias = await catalogoService.obtenerCategorias(true);
        this.ubicaciones = await catalogoService.obtenerUbicaciones(true);
      } catch (e) {}
    },
    async cargarDatos() {
      this.cargando = true;
      try {
        this.metrics = await ticketService.obtenerDashboardAdmin();
        this.pagedResult = await ticketService.obtenerTickets(this.filtros);
      } catch (e) {
        console.error(e);
      } finally {
        this.cargando = false;
      }
    },
    aplicarFiltros() {
      this.filtros.pagina = 1;
      this.cargarDatos();
    },
    limpiarFiltros() {
      this.filtros = { busqueda: '', estado: '', prioridad: '', categoriaId: '', ubicacionId: '', orden: '', pagina: 1 };
      this.cargarDatos();
    },
    cambiarPagina(p) {
      if (p < 1 || p > this.pagedResult.totalPages) return;
      this.filtros.pagina = p;
      this.cargarDatos();
    },
    exportarCsv() {
      ticketService.descargarExportacionCsv(this.filtros);
    }
  },
  template: `
    <div>
      <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3 mb-4">
        <div>
          <h3 class="fw-bold mb-0 text-dark">Panel de Control TI (Vue 3 + vue-chartjs)</h3>
          <p class="text-muted mb-0 small">Métricas analíticas, auditoría y supervisión en tiempo real</p>
        </div>
        <div class="d-flex flex-wrap gap-2">
          <button @click="exportarCsv" class="btn btn-outline-success rounded-3 fw-semibold"><i class="bi bi-file-earmark-spreadsheet me-1"></i>Exportar CSV</button>
          <button @click="$emit('navigate', 'gestion-tecnicos')" class="btn btn-outline-primary rounded-3 fw-semibold"><i class="bi bi-people-fill me-1"></i>Técnicos</button>
          <button @click="$emit('navigate', 'categorias')" class="btn btn-outline-secondary rounded-3 fw-semibold"><i class="bi bi-tags-fill me-1"></i>Catálogos</button>
          <button @click="$emit('navigate', 'crear-ticket')" class="btn btn-tecnm-primary rounded-3"><i class="bi bi-plus-lg me-1"></i>Nuevo Ticket</button>
        </div>
      </div>

      <!-- KPIs -->
      <div class="row g-3 mb-4" v-if="metrics">
        <div class="col-6 col-lg-3">
          <div class="card card-tecnm p-3 border-0 shadow-sm">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <div class="text-muted small fw-bold text-uppercase">Total Tickets</div>
                <div class="h2 fw-extrabold mb-0 text-dark">{{ metrics.totalTickets }}</div>
              </div>
              <div class="stat-icon-wrapper bg-primary bg-opacity-10 text-primary"><i class="bi bi-ticket-detailed-fill"></i></div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card card-tecnm p-3 border-0 shadow-sm">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <div class="text-muted small fw-bold text-uppercase">Abiertos</div>
                <div class="h2 fw-extrabold mb-0 text-warning">{{ metrics.ticketsAbiertos }}</div>
              </div>
              <div class="stat-icon-wrapper bg-warning bg-opacity-10 text-warning"><i class="bi bi-hourglass-split"></i></div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card card-tecnm p-3 border-0 shadow-sm">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <div class="text-muted small fw-bold text-uppercase">En Progreso</div>
                <div class="h2 fw-extrabold mb-0 text-info">{{ metrics.ticketsEnProgreso }}</div>
              </div>
              <div class="stat-icon-wrapper bg-info bg-opacity-10 text-info"><i class="bi bi-gear-wide-connected"></i></div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card card-tecnm p-3 border-0 shadow-sm">
            <div class="d-flex align-items-center justify-content-between">
              <div>
                <div class="text-muted small fw-bold text-uppercase">Resueltos</div>
                <div class="h2 fw-extrabold mb-0 text-success">{{ metrics.ticketsResueltos }}</div>
              </div>
              <div class="stat-icon-wrapper bg-success bg-opacity-10 text-success"><i class="bi bi-check2-circle"></i></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts Reutilizables con vue-chartjs -->
      <div class="row g-4 mb-4" v-if="metrics">
        <div class="col-lg-4">
          <div class="card card-tecnm h-100 border-0 shadow-sm p-3">
            <h6 class="fw-bold text-dark border-bottom pb-2"><i class="bi bi-pie-chart-fill text-primary me-2"></i>Estado General</h6>
            <div style="height: 220px; position: relative;">
              <doughnut-chart v-if="chartEstadosData" :chart-data="chartEstadosData" />
            </div>
          </div>
        </div>
        <div class="col-lg-4">
          <div class="card card-tecnm h-100 border-0 shadow-sm p-3">
            <h6 class="fw-bold text-dark border-bottom pb-2"><i class="bi bi-bar-chart-fill text-danger me-2"></i>Fallas por Edificio</h6>
            <div style="height: 220px; position: relative;">
              <bar-chart v-if="chartUbicacionesData" :chart-data="chartUbicacionesData" />
            </div>
          </div>
        </div>
        <div class="col-lg-4">
          <div class="card card-tecnm h-100 border-0 shadow-sm p-3">
            <h6 class="fw-bold text-dark border-bottom pb-2"><i class="bi bi-tags-fill text-warning me-2"></i>Fallas por Categoría</h6>
            <div style="height: 220px; position: relative;">
              <bar-chart v-if="chartCategoriasData" :chart-data="chartCategoriasData" :chart-options="chartCategoriasOptions" />
            </div>
          </div>
        </div>
      </div>

      <!-- Tabla y Filtros -->
      <div class="card card-tecnm border-0 shadow-sm overflow-hidden mb-4">
        <div class="card-header bg-white py-3 border-bottom">
          <form @submit.prevent="aplicarFiltros" class="row g-2 align-items-center">
            <div class="col-12 col-md-3">
              <input type="text" v-model="filtros.busqueda" class="form-control form-control-sm" placeholder="Buscar folio, solicitante, aula..." />
            </div>
            <div class="col-6 col-md-2">
              <select v-model="filtros.estado" class="form-select form-select-sm">
                <option value="">-- Estado --</option>
                <option value="Abierto">Abierto</option>
                <option value="EnProgreso">En Progreso</option>
                <option value="Resuelto">Resuelto</option>
                <option value="Cancelado">Cancelado</option>
              </select>
            </div>
            <div class="col-6 col-md-2">
              <select v-model="filtros.prioridad" class="form-select form-select-sm">
                <option value="">-- Prioridad --</option>
                <option value="Alta">Alta</option>
                <option value="Normal">Normal</option>
                <option value="Urgente">Urgente</option>
              </select>
            </div>
            <div class="col-6 col-md-2">
              <select v-model="filtros.categoriaId" class="form-select form-select-sm">
                <option value="">-- Categoría --</option>
                <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
              </select>
            </div>
            <div class="col-6 col-md-2">
              <select v-model="filtros.orden" class="form-select form-select-sm">
                <option value="">Recientes</option>
                <option value="prioridad_desc">Prioridad Alta</option>
                <option value="folio_asc">Folio Asc</option>
              </select>
            </div>
            <div class="col-12 col-md-1 d-flex gap-1">
              <button type="submit" class="btn btn-sm btn-tecnm-primary w-100"><i class="bi bi-filter"></i></button>
              <button type="button" @click="limpiarFiltros" class="btn btn-sm btn-outline-secondary"><i class="bi bi-x-lg"></i></button>
            </div>
          </form>
        </div>

        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-tecnm table-hover mb-0">
              <thead>
                <tr>
                  <th>Folio</th>
                  <th>Problema / Falla</th>
                  <th>Solicitante</th>
                  <th>Ubicación</th>
                  <th>Técnico</th>
                  <th>Prioridad</th>
                  <th>Estado</th>
                  <th class="text-end">Acción</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="!pagedResult.items.length"><td colspan="8" class="text-center text-muted py-4">No se encontraron tickets con los filtros aplicados.</td></tr>
                <tr v-for="t in pagedResult.items" :key="t.id">
                  <td class="fw-bold text-primary">#{{ t.id }}</td>
                  <td>
                    <a @click="$emit('navigate', 'detalle-ticket', t.id)" class="fw-bold text-dark text-decoration-none" style="cursor:pointer;">{{ t.titulo }}</a>
                    <small class="text-muted d-block text-truncate" style="max-width: 220px;">{{ t.descripcion }}</small>
                  </td>
                  <td>
                    <div class="fw-semibold small">{{ t.solicitante ? t.solicitante.nombreCompleto : 'N/A' }}</div>
                    <small class="text-muted">{{ t.solicitante ? t.solicitante.email : '' }}</small>
                  </td>
                  <td>
                    <span class="fw-semibold">{{ t.ubicacion ? t.ubicacion.nombre : 'N/A' }}</span>
                    <small class="text-muted d-block" v-if="t.detalleAula">{{ t.detalleAula }}</small>
                  </td>
                  <td>
                    <span v-if="t.tecnicoAsignado" class="small fw-semibold text-dark"><i class="bi bi-person-check me-1 text-success"></i>{{ t.tecnicoAsignado.nombreCompleto }}</span>
                    <span v-else class="badge bg-warning text-dark"><i class="bi bi-clock me-1"></i>Sin asignar</span>
                  </td>
                  <td>
                    <span v-if="t.prioridad === 'Alta' || t.prioridad === 2" class="badge-priority-alta"><i class="bi bi-lightning-charge-fill me-1"></i>Alta</span>
                    <span v-else-if="t.prioridad === 'Urgente' || t.prioridad === 3" class="badge bg-danger text-white rounded-pill px-2 py-1"><i class="bi bi-exclamation-triangle-fill me-1"></i>Urgente</span>
                    <span v-else-if="t.prioridad === 'Baja' || t.prioridad === 0" class="badge bg-secondary text-white rounded-pill px-2 py-1">Baja</span>
                    <span v-else class="badge-priority-normal">Normal</span>
                  </td>
                  <td>
                    <span v-if="t.estado === 'Abierto' || t.estado === 0" class="badge-status-abierto"><i class="bi bi-hourglass-split me-1"></i>Abierto</span>
                    <span v-else-if="t.estado === 'EnProgreso' || t.estado === 1" class="badge-status-progreso"><i class="bi bi-gear-fill me-1"></i>En Progreso</span>
                    <span v-else-if="t.estado === 'Resuelto' || t.estado === 2" class="badge-status-resuelto"><i class="bi bi-check2-all me-1"></i>Resuelto</span>
                    <span v-else class="badge bg-secondary text-white rounded-pill px-2 py-1">Cancelado</span>
                  </td>
                  <td class="text-end">
                    <button @click="$emit('navigate', 'detalle-ticket', t.id)" class="btn btn-sm btn-outline-primary rounded-2"><i class="bi bi-eye-fill me-1"></i>Ver</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Paginación -->
        <div class="card-footer bg-white py-3 border-top d-flex justify-content-between align-items-center" v-if="pagedResult.totalPages > 1">
          <small class="text-muted">Página {{ pagedResult.pageNumber }} de {{ pagedResult.totalPages }} (Total: {{ pagedResult.totalItems }})</small>
          <ul class="pagination pagination-sm mb-0">
            <li class="page-item" :class="{ disabled: !pagedResult.hasPreviousPage }">
              <a class="page-link" @click="cambiarPagina(pagedResult.pageNumber - 1)" style="cursor:pointer;"><i class="bi bi-chevron-left"></i> Anterior</a>
            </li>
            <li v-for="p in pagedResult.totalPages" :key="p" class="page-item" :class="{ active: p === pagedResult.pageNumber }">
              <a class="page-link" @click="cambiarPagina(p)" style="cursor:pointer;">{{ p }}</a>
            </li>
            <li class="page-item" :class="{ disabled: !pagedResult.hasNextPage }">
              <a class="page-link" @click="cambiarPagina(pagedResult.pageNumber + 1)" style="cursor:pointer;">Siguiente <i class="bi bi-chevron-right"></i></a>
            </li>
          </ul>
        </div>
      </div>
    </div>
  `
};
