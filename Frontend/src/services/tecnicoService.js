import { fetchApi } from './apiConfig.js';

export const tecnicoService = {
  async obtenerTecnicos() {
    return await fetchApi('/account/tecnicos');
  },

  async crearTecnico(nombreCompleto, email, password, confirmPassword) {
    return await fetchApi('/account/crear-tecnico', {
      method: 'POST',
      body: JSON.stringify({ nombreCompleto, email, password, confirmPassword })
    });
  },

  async alternarEstadoTecnico(id) {
    return await fetchApi(`/account/tecnicos/${id}/alternar-estado`, {
      method: 'POST'
    });
  }
};
