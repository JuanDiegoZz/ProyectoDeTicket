import { ticketService } from '../services/ticketService.js';
import { tecnicoService } from '../services/tecnicoService.js';

export default {
  name: 'DetalleTicketView',
  props: {
    ticketId: Number,
    usuarioActual: Object
  },
  emits: ['navigate'],
  data() {
    return {
      ticket: null,
      tecnicos: [],
      nuevoEstado: 'Abierto',
      notaEstado: '',
      nuevoTecnicoId: '',
      motivoReasignacion: '',
      nuevaPrioridad: 'Normal',
      estrellas: 5,
      comentarioCalificacion: '',
      nuevaNotaMensaje: '',
      cargando: true,
      mensajeAccion: null,
      errorAccion: null
    };
  },
  async created() {
    await this.cargarDetalle();
    if (this.usuarioActual && this.usuarioActual.rol === 'Administrador') {
      await this.cargarTecnicos();
    }
  },
  methods: {
    async cargarDetalle() {
      this.cargando = true;
      try {
        this.ticket = await ticketService.obtenerDetalle(this.ticketId);
        this.nuevoEstado = this.ticket.estado;
        this.nuevaPrioridad = this.ticket.prioridad;
      } catch (e) {
        this.errorAccion = e.message || 'No se pudo cargar el ticket.';
      } finally {
        this.cargando = false;
      }
    },
    async cargarTecnicos() {
      try {
        this.tecnicos = await tecnicoService.obtenerTecnicos();
      } catch (e) {}
    },
    async cambiarEstado() {
      this.errorAccion = null;
      try {
        await ticketService.cambiarEstado(this.ticketId, this.nuevoEstado, this.notaEstado);
        this.mensajeAccion = 'Estado actualizado correctamente.';
        this.notaEstado = '';
        await this.cargarDetalle();
      } catch (e) {
        this.errorAccion = e.message;
      }
    },
    async reasignarTecnico() {
      this.errorAccion = null;
      try {
        await ticketService.reasignarTecnico(this.ticketId, this.nuevoTecnicoId ? parseInt(this.nuevoTecnicoId) : null, this.motivoReasignacion);
        this.mensajeAccion = 'Técnico reasignado exitosamente.';
        this.motivoReasignacion = '';
        await this.cargarDetalle();
      } catch (e) {
        this.errorAccion = e.message;
      }
    },
    async cambiarPrioridad() {
      this.errorAccion = null;
      try {
        await ticketService.cambiarPrioridad(this.ticketId, this.nuevaPrioridad);
        this.mensajeAccion = 'Prioridad actualizada correctamente.';
        await this.cargarDetalle();
      } catch (e) {
        this.errorAccion = e.message;
      }
    },
    async calificar() {
      this.errorAccion = null;
      try {
        await ticketService.calificarTicket(this.ticketId, this.estrellas, this.comentarioCalificacion);
        this.mensajeAccion = '¡Gracias por evaluar nuestro servicio!';
        await this.cargarDetalle();
      } catch (e) {
        this.errorAccion = e.message;
      }
    },
    async agregarNota() {
      this.errorAccion = null;
      if (!this.nuevaNotaMensaje.trim()) return;
      try {
        await ticketService.agregarNota(this.ticketId, this.nuevaNotaMensaje);
        this.mensajeAccion = 'Nota agregada a la bitácora.';
        this.nuevaNotaMensaje = '';
        await this.cargarDetalle();
      } catch (e) {
        this.errorAccion = e.message;
      }
    }
  },
  template: `
    <div v-if="cargando" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>
    <div v-else-if="ticket">
      <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3 mb-4">
        <div>
          <div class="d-flex align-items-center gap-2 mb-1">
            <h3 class="fw-bold mb-0 text-dark">Ticket #{{ ticket.id }}</h3>
            <span v-if="ticket.prioridad === 'Alta' || ticket.prioridad === 2" class="badge-priority-alta"><i class="bi bi-lightning-fill me-1"></i>Alta</span>
            <span v-else-if="ticket.prioridad === 'Urgente' || ticket.prioridad === 3" class="badge bg-danger text-white rounded-pill px-2 py-1"><i class="bi bi-exclamation-triangle-fill me-1"></i>Urgente</span>
            <span v-else class="badge-priority-normal">Normal</span>

            <span v-if="ticket.estado === 'Abierto' || ticket.estado === 0" class="badge-status-abierto">Abierto</span>
            <span v-else-if="ticket.estado === 'EnProgreso' || ticket.estado === 1" class="badge-status-progreso">En Progreso</span>
            <span v-else-if="ticket.estado === 'Resuelto' || ticket.estado === 2" class="badge-status-resuelto">Resuelto</span>
            <span v-else class="badge bg-secondary text-white rounded-pill px-2 py-1">Cancelado</span>
          </div>
          <p class="text-muted small mb-0">{{ ticket.titulo }}</p>
        </div>
        <button @click="$emit('navigate', 'home')" class="btn btn-outline-secondary rounded-3"><i class="bi bi-arrow-left me-1"></i>Volver</button>
      </div>

      <div v-if="mensajeAccion" class="alert alert-success alert-dismissible fade show"><i class="bi bi-check-circle-fill me-2"></i>{{ mensajeAccion }}</div>
      <div v-if="errorAccion" class="alert alert-danger alert-dismissible fade show"><i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorAccion }}</div>

      <div class="row g-4">
        <!-- Columna Izquierda: Información de la Incidencia -->
        <div class="col-lg-8">
          <div class="card card-tecnm border-0 shadow-sm mb-4">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-info-circle-fill text-primary me-2"></i>Detalle de la Falla</h6></div>
            <div class="card-body p-4">
              <div class="row g-3 mb-4">
                <div class="col-6 col-md-3">
                  <span class="text-muted small d-block">Solicitante:</span>
                  <span class="fw-semibold text-dark">{{ ticket.solicitante ? ticket.solicitante.nombreCompleto : 'N/A' }}</span>
                </div>
                <div class="col-6 col-md-3">
                  <span class="text-muted small d-block">Categoría:</span>
                  <span class="badge bg-light text-dark border">{{ ticket.categoria ? ticket.categoria.nombre : 'N/A' }}</span>
                </div>
                <div class="col-6 col-md-3">
                  <span class="text-muted small d-block">Ubicación / Aula:</span>
                  <span class="fw-semibold text-dark">{{ ticket.ubicacion ? ticket.ubicacion.nombre : 'N/A' }} ({{ ticket.detalleAula || 'N/A' }})</span>
                </div>
                <div class="col-6 col-md-3">
                  <span class="text-muted small d-block">Técnico Asignado:</span>
                  <span class="fw-semibold text-dark">{{ ticket.tecnicoAsignado ? ticket.tecnicoAsignado.nombreCompleto : 'Sin Asignar' }}</span>
                </div>
              </div>

              <h6 class="fw-bold text-dark mb-2">Descripción del Problema:</h6>
              <div class="p-3 bg-light rounded-3 text-secondary mb-4" style="white-space: pre-wrap;">{{ ticket.descripcion }}</div>

              <div v-if="ticket.rutaEvidencia" class="mb-4">
                <h6 class="fw-bold text-dark mb-2">Evidencia Adjunta:</h6>
                <a :href="ticket.rutaEvidencia" target="_blank" class="btn btn-sm btn-outline-primary"><i class="bi bi-paperclip me-1"></i>Ver Archivo Adjunto</a>
              </div>
            </div>
          </div>

          <!-- Bitácora de Notas -->
          <div class="card card-tecnm border-0 shadow-sm mb-4">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-journal-text text-info me-2"></i>Bitácora de Seguimiento y Notas</h6></div>
            <div class="card-body p-4">
              <div v-if="!ticket.notas.length" class="text-muted small text-center py-3">No hay notas registradas en este ticket aún.</div>
              <div v-for="n in ticket.notas" :key="n.id" class="p-3 border-bottom mb-3">
                <div class="d-flex justify-content-between align-items-center mb-1">
                  <span class="fw-bold text-dark small">{{ n.usuario ? n.usuario.nombreCompleto : 'Sistema' }}</span>
                  <small class="text-muted">{{ new Date(n.fechaCreacion).toLocaleString() }}</small>
                </div>
                <p class="mb-0 text-secondary small">{{ n.mensaje }}</p>
              </div>

              <!-- Agregar Nota Form -->
              <form @submit.prevent="agregarNota" class="mt-4">
                <div class="input-group">
                  <input type="text" v-model="nuevaNotaMensaje" class="form-control form-control-custom" placeholder="Escribe un comentario o actualización..." required />
                  <button type="submit" class="btn btn-tecnm-primary"><i class="bi bi-send-fill me-1"></i>Agregar Nota</button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- Columna Derecha: Acciones por Rol -->
        <div class="col-lg-4">
          <!-- Acción Técnico/Admin: Cambiar Estado -->
          <div class="card card-tecnm border-0 shadow-sm mb-4" v-if="usuarioActual.rol === 'Tecnico' || usuarioActual.rol === 'Administrador'">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-gear-fill me-2 text-primary"></i>Actualizar Estado</h6></div>
            <div class="card-body p-3">
              <form @submit.prevent="cambiarEstado">
                <div class="mb-3">
                  <label class="form-label small text-uppercase fw-semibold text-secondary">Nuevo Estado</label>
                  <select v-model="nuevoEstado" class="form-select form-select-custom">
                    <option value="Abierto">Abierto</option>
                    <option value="EnProgreso">En Progreso</option>
                    <option value="Resuelto">Resuelto</option>
                    <option value="Cancelado">Cancelado</option>
                  </select>
                </div>
                <div class="mb-3">
                  <label class="form-label small text-uppercase fw-semibold text-secondary">Nota del Cambio</label>
                  <input type="text" v-model="notaEstado" class="form-control form-control-custom" placeholder="Comentario opcional..." />
                </div>
                <button type="submit" class="btn btn-tecnm-primary w-100 fw-bold"><i class="bi bi-check2-circle me-1"></i>Guardar Estado</button>
              </form>
            </div>
          </div>

          <!-- Acción Admin: Cambiar Prioridad -->
          <div class="card card-tecnm border-0 shadow-sm mb-4" v-if="usuarioActual.rol === 'Administrador'">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-flag-fill text-warning me-2"></i>Gestión de Prioridad (Admin)</h6></div>
            <div class="card-body p-3">
              <form @submit.prevent="cambiarPrioridad">
                <div class="mb-3">
                  <select v-model="nuevaPrioridad" class="form-select form-select-custom">
                    <option value="Baja">Baja</option>
                    <option value="Normal">Normal</option>
                    <option value="Alta">Alta</option>
                    <option value="Urgente">Urgente</option>
                  </select>
                </div>
                <button type="submit" class="btn btn-outline-warning w-100 fw-bold"><i class="bi bi-check2 me-1"></i>Actualizar Prioridad</button>
              </form>
            </div>
          </div>

          <!-- Acción Admin: Reasignar Técnico -->
          <div class="card card-tecnm border-0 shadow-sm mb-4" v-if="usuarioActual.rol === 'Administrador'">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-person-gear text-primary me-2"></i>Reasignar Técnico</h6></div>
            <div class="card-body p-3">
              <form @submit.prevent="reasignarTecnico">
                <div class="mb-3">
                  <select v-model="nuevoTecnicoId" class="form-select form-select-custom">
                    <option value="">-- Liberar a Cola General --</option>
                    <option v-for="t in tecnicos" :key="t.id" :value="t.id">{{ t.nombreCompleto }}</option>
                  </select>
                </div>
                <div class="mb-3">
                  <input type="text" v-model="motivoReasignacion" class="form-control form-control-custom" placeholder="Motivo de reasignación..." />
                </div>
                <button type="submit" class="btn btn-outline-primary w-100 fw-bold"><i class="bi bi-arrow-repeat me-1"></i>Reasignar Ticket</button>
              </form>
            </div>
          </div>

          <!-- Acción Solicitante: Calificar Ticket -->
          <div class="card card-tecnm border-0 shadow-sm mb-4" v-if="ticket.estado === 'Resuelto' && usuarioActual.id === ticket.solicitanteId">
            <div class="card-header bg-white py-3 border-bottom"><h6 class="fw-bold mb-0 text-dark"><i class="bi bi-star-fill text-warning me-2"></i>Calificación de Satisfacción</h6></div>
            <div class="card-body p-3">
              <div v-if="ticket.calificacionSatisfaccion" class="text-center py-2">
                <div class="fs-3 text-warning mb-1">
                  <i v-for="s in ticket.calificacionSatisfaccion" :key="s" class="bi bi-star-fill me-1"></i>
                </div>
                <p class="text-muted small mb-0">{{ ticket.comentarioSatisfaccion || 'Sin comentarios adicionales.' }}</p>
              </div>
              <form v-else @submit.prevent="calificar">
                <div class="mb-3 text-center">
                  <label class="form-label small fw-semibold text-secondary">¿Cómo evalúas la atención técnica?</label>
                  <select v-model="estrellas" class="form-select form-select-custom">
                    <option :value="5">⭐⭐⭐⭐⭐ Excelencia (5 Estrellas)</option>
                    <option :value="4">⭐⭐⭐⭐ Muy Bueno (4 Estrellas)</option>
                    <option :value="3">⭐⭐⭐ Aceptable (3 Estrellas)</option>
                    <option :value="2">⭐⭐ Deficiente (2 Estrellas)</option>
                    <option :value="1">⭐ Incomodo / Malo (1 Estrella)</option>
                  </select>
                </div>
                <div class="mb-3">
                  <textarea v-model="comentarioCalificacion" class="form-control form-control-custom" rows="2" placeholder="Escribe un comentario opcional sobre la atención..."></textarea>
                </div>
                <button type="submit" class="btn btn-tecnm-gold w-100 fw-bold"><i class="bi bi-send me-1"></i>Enviar Calificación</button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
};
