import { fetchApi } from './apiConfig.js';

export const catalogoService = {
  async obtenerCategorias(soloActivas = true) {
    return await fetchApi(`/catalogos/categorias?soloActivas=${soloActivas}`);
  },

  async crearCategoria(categoria) {
    return await fetchApi('/catalogos/categorias', {
      method: 'POST',
      body: JSON.stringify(categoria)
    });
  },

  async editarCategoria(id, categoria) {
    return await fetchApi(`/catalogos/categorias/${id}`, {
      method: 'PUT',
      body: JSON.stringify(categoria)
    });
  },

  async alternarEstadoCategoria(id) {
    return await fetchApi(`/catalogos/categorias/${id}/alternar-estado`, {
      method: 'POST'
    });
  },

  async obtenerUbicaciones(soloActivas = true) {
    return await fetchApi(`/catalogos/ubicaciones?soloActivas=${soloActivas}`);
  },

  async crearUbicacion(ubicacion) {
    return await fetchApi('/catalogos/ubicaciones', {
      method: 'POST',
      body: JSON.stringify(ubicacion)
    });
  },

  async editarUbicacion(id, ubicacion) {
    return await fetchApi(`/catalogos/ubicaciones/${id}`, {
      method: 'PUT',
      body: JSON.stringify(ubicacion)
    });
  },

  async alternarEstadoUbicacion(id) {
    return await fetchApi(`/catalogos/ubicaciones/${id}/alternar-estado`, {
      method: 'POST'
    });
  }
};
