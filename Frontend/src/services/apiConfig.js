// Configuración Centralizada de la API REST .NET 10
export const API_BASE_URL = 'http://localhost:5000/api';

export async function fetchApi(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const defaultHeaders = {
    'Accept': 'application/json'
  };

  if (!(options.body instanceof FormData)) {
    defaultHeaders['Content-Type'] = 'application/json';
  }

  const config = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers
    },
    credentials: 'include' // Enviar cookies de sesión
  };

  try {
    const response = await fetch(url, config);
    
    if (response.status === 401) {
      window.dispatchEvent(new CustomEvent('auth-unauthorized'));
      throw new Error('Sesión expirada o no autorizada.');
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.mensaje || `Error HTTP ${response.status}`);
    }

    // Si es descarga de archivo CSV
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('text/csv')) {
      return await response.blob();
    }

    return await response.json();
  } catch (error) {
    console.error(`Error de red en endpoint [${endpoint}]:`, error);
    throw error;
  }
}
