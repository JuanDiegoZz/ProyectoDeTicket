import { fetchApi } from './apiConfig.js';

export const authService = {
  async login(email, password) {
    return await fetchApi('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
  },

  async registro(nombreCompleto, email, password, confirmPassword) {
    return await fetchApi('/auth/registro', {
      method: 'POST',
      body: JSON.stringify({ nombreCompleto, email, password, confirmPassword })
    });
  },

  async logout() {
    return await fetchApi('/auth/logout', {
      method: 'POST'
    });
  },

  async obtenerUsuarioActual() {
    return await fetchApi('/auth/me');
  }
};
