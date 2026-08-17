const Api = {
  get baseUrl() {
    return (localStorage.getItem(APP_CONFIG.storageKey) || APP_CONFIG.apiBaseUrl).replace(/\/$/, "");
  },

  async request(endpoint, options = {}) {
    const request = async baseUrl => fetch(`${baseUrl}/${endpoint}`, {
      headers: { "Content-Type": "application/json", ...options.headers },
      ...options
    });
    const configuredUrl = this.baseUrl;
    let response;

    try {
      response = await request(configuredUrl);
    } catch (error) {
      // Una URL anterior puede quedar guardada en el navegador aunque el backend
      // vuelva a usar la URL predeterminada del proyecto.
      if (configuredUrl === APP_CONFIG.apiBaseUrl) throw error;

      response = await request(APP_CONFIG.apiBaseUrl);
      localStorage.removeItem(APP_CONFIG.storageKey);
    }

    if (!response.ok) {
      let message = `La API respondió con el código ${response.status}.`;
      try {
        const detail = await response.json();
        message = detail.title || detail.message || detail.errors?.[Object.keys(detail.errors)[0]]?.[0] || message;
      } catch (_) { /* La API puede devolver un cuerpo vacío. */ }
      throw new Error(message);
    }
    if (response.status === 204 || response.status === 201) return null;
    return response.json();
  },

  list(endpoint) { return this.request(endpoint); },
  search(endpoint, texto) { return this.request(`${endpoint}/buscar?texto=${encodeURIComponent(texto)}`); },
  create(endpoint, payload) { return this.request(endpoint, { method: "POST", body: JSON.stringify(payload) }); },
  update(endpoint, id, payload) { return this.request(`${endpoint}/${id}`, { method: "PUT", body: JSON.stringify(payload) }); },
  remove(endpoint, id) { return this.request(`${endpoint}/${id}`, { method: "DELETE" }); }
};
