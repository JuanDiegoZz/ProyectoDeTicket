import { ticketService } from '../services/ticketService.js';

export default {
  name: 'TicketsTecnicoView',
  emits: ['navigate'],
  data() {
    return {
      tickets: [],
      cargando: true
    };
  },
  async created() {
    await this.cargarTickets();
  },
  methods: {
    async cargarTickets() {
      this.cargando = true;
      try {
        this.tickets = await ticketService.obtenerTickets();
      } catch (e) {
        console.error(e);
      } finally {
        this.cargando = false;
      }
    }
  },
  template: `
    <div>
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 class="fw-bold mb-0 text-dark">Cola de Atención Técnica</h3>
          <p class="text-muted small mb-0">Tickets asignados a tu usuario o disponibles en la cola general</p>
        </div>
      </div>

      <div class="card card-tecnm border-0 shadow-sm overflow-hidden">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-tecnm table-hover mb-0">
              <thead>
                <tr>
                  <th>Folio</th>
                  <th>Incidencia</th>
                  <th>Solicitante</th>
                  <th>Ubicación</th>
                  <th>Prioridad</th>
                  <th>Estado</th>
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="!tickets.length"><td colspan="7" class="text-center text-muted py-4">No hay tickets asignados o disponibles en este momento.</td></tr>
                <tr v-for="t in tickets" :key="t.id" :class="{'table-danger-subtle': t.prioridad === 'Alta'}">
                  <td class="fw-bold text-primary">#{{ t.id }}</td>
                  <td>
                    <a @click="$emit('navigate', 'detalle-ticket', t.id)" class="fw-bold text-dark text-decoration-none" style="cursor:pointer;">{{ t.titulo }}</a>
                    <small class="text-muted d-block text-truncate" style="max-width: 250px;">{{ t.descripcion }}</small>
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
                    <span v-if="t.prioridad === 'Alta' || t.prioridad === 2" class="badge-priority-alta"><i class="bi bi-lightning-charge-fill me-1"></i>Alta (Docente)</span>
                    <span v-else-if="t.prioridad === 'Urgente' || t.prioridad === 3" class="badge bg-danger text-white rounded-pill px-2 py-1"><i class="bi bi-exclamation-triangle-fill me-1"></i>Urgente</span>
                    <span v-else class="badge-priority-normal">Normal</span>
                  </td>
                  <td>
                    <span v-if="t.estado === 'Abierto' || t.estado === 0" class="badge-status-abierto">Abierto</span>
                    <span v-else-if="t.estado === 'EnProgreso' || t.estado === 1" class="badge-status-progreso">En Progreso</span>
                    <span v-else-if="t.estado === 'Resuelto' || t.estado === 2" class="badge-status-resuelto">Resuelto</span>
                    <span v-else class="badge bg-secondary text-white rounded-pill px-2 py-1">Cancelado</span>
                  </td>
                  <td class="text-end">
                    <button @click="$emit('navigate', 'detalle-ticket', t.id)" class="btn btn-sm btn-tecnm-primary rounded-2"><i class="bi bi-tools me-1"></i>Atender</button>
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
