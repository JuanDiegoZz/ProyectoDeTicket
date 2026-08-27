import { fetchApi } from './apiConfig.js';

export const ticketService = {
  async obtenerTickets(params = {}) {
    const query = new URLSearchParams();
    if (params.busqueda) query.append('busqueda', params.busqueda);
    if (params.estado) query.append('estado', params.estado);
    if (params.prioridad) query.append('prioridad', params.prioridad);
    if (params.categoriaId) query.append('categoriaId', params.categoriaId);
    if (params.ubicacionId) query.append('ubicacionId', params.ubicacionId);
    if (params.fechaInicio) query.append('fechaInicio', params.fechaInicio);
    if (params.fechaFin) query.append('fechaFin', params.fechaFin);
    if (params.pagina) query.append('pagina', params.pagina);
    if (params.tamanoPagina) query.append('tamanoPagina', params.tamanoPagina);
    if (params.orden) query.append('orden', params.orden);

    return await fetchApi(`/tickets?${query.toString()}`);
  },

  async obtenerDashboardAdmin() {
    return await fetchApi('/tickets/dashboard-admin');
  },

  async obtenerDetalle(id) {
    return await fetchApi(`/tickets/${id}`);
  },

  async crearTicket(formData) {
    return await fetchApi('/tickets', {
      method: 'POST',
      body: formData
    });
  },

  async cambiarEstado(id, nuevoEstado, nota) {
    return await fetchApi(`/tickets/${id}/cambiar-estado`, {
      method: 'POST',
      body: JSON.stringify({ nuevoEstado, nota })
    });
  },

  async reasignarTecnico(id, nuevoTecnicoId, motivo) {
    return await fetchApi(`/tickets/${id}/reasignar`, {
      method: 'POST',
      body: JSON.stringify({ nuevoTecnicoId, motivo })
    });
  },

  async cambiarPrioridad(id, nuevaPrioridad) {
    return await fetchApi(`/tickets/${id}/cambiar-prioridad`, {
      method: 'POST',
      body: JSON.stringify({ nuevaPrioridad })
    });
  },

  async calificarTicket(id, estrellas, comentario) {
    return await fetchApi(`/tickets/${id}/calificar`, {
      method: 'POST',
      body: JSON.stringify({ estrellas, comentario })
    });
  },

  async agregarNota(id, mensaje) {
    return await fetchApi(`/tickets/${id}/notas`, {
      method: 'POST',
      body: JSON.stringify({ mensaje })
    });
  },

  async obtenerDashboardAdmin(params = {}) {
    const query = new URLSearchParams();
    if (params.busqueda) query.append('busqueda', params.busqueda);
    if (params.estado) query.append('estado', params.estado);
    if (params.prioridad) query.append('prioridad', params.prioridad);
    if (params.categoriaId) query.append('categoriaId', params.categoriaId);
    if (params.ubicacionId) query.append('ubicacionId', params.ubicacionId);
    if (params.fechaInicio) query.append('fechaInicio', params.fechaInicio);
    if (params.fechaFin) query.append('fechaFin', params.fechaFin);

    const queryString = query.toString() ? `?${query.toString()}` : '';
    return await fetchApi(`/tickets/dashboard-admin${queryString}`);
  },

  async descargarExportacionCsv(params = {}) {
    const query = new URLSearchParams();
    if (params.busqueda) query.append('busqueda', params.busqueda);
    if (params.estado) query.append('estado', params.estado);
    if (params.prioridad) query.append('prioridad', params.prioridad);
    if (params.categoriaId) query.append('categoriaId', params.categoriaId);
    if (params.ubicacionId) query.append('ubicacionId', params.ubicacionId);
    if (params.fechaInicio) query.append('fechaInicio', params.fechaInicio);
    if (params.fechaFin) query.append('fechaFin', params.fechaFin);
    if (params.orden) query.append('orden', params.orden);

    const blob = await fetchApi(`/tickets/exportar-csv?${query.toString()}`);
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Reporte_Tickets_TecNM_${new Date().toISOString().slice(0,10)}.csv`;
    document.body.appendChild(a);
    a.click();
    a.remove();
  }
};
