const Api = {
  get baseUrl() {
    return (localStorage.getItem(APP_CONFIG.storageKey) || APP_CONFIG.apiBaseUrl).replace(/\/$/, "");
  },

  get accessToken() {
    return localStorage.getItem(APP_CONFIG.tokenKey) || "";
  },

  setSession(auth) {
    if (!auth?.accessToken) return;
    localStorage.setItem(APP_CONFIG.tokenKey, auth.accessToken);
    if (auth.refreshToken) localStorage.setItem(APP_CONFIG.refreshTokenKey, auth.refreshToken);
  },

  clearSession() {
    localStorage.removeItem(APP_CONFIG.tokenKey);
    localStorage.removeItem(APP_CONFIG.refreshTokenKey);
  },

  async request(endpoint, options = {}) {
    const request = async baseUrl => {
      const headers = { "Content-Type": "application/json", ...options.headers };
      if (this.accessToken && !headers.Authorization) {
        headers.Authorization = `Bearer ${this.accessToken}`;
      }
      const cleanEndpoint = endpoint.replace(/^\/+/, "");
      return fetch(`${baseUrl}/${cleanEndpoint}`, {
        ...options,
        headers
      });
    };
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

  if (response.status === 401) {
    this.clearSession();
    window.location.href = "Login.html";
    throw new Error("Sesión expirada o no autorizada. Por favor, inicia sesión.");
  }

  let message = `La API respondió con el código ${response.status}.`;
      try {
        const detail = await response.json();
        message = detail.title || detail.message || detail.errors?.[Object.keys(detail.errors)[0]]?.[0] || message;
      } catch (_) { }
      throw new Error(message);
    }
    if (response.status === 204 ) return null;
    return response.json();
  },

  list(endpoint) { return this.request(endpoint); },
  search(endpoint, texto) { return this.request(`${endpoint}/buscar?texto=${encodeURIComponent(texto)}`); },
  create(endpoint, payload) { return this.request(endpoint, { method: "POST", body: JSON.stringify(payload) }); },
  update(endpoint, id, payload) { return this.request(`${endpoint}/${id}`, { method: "PUT", body: JSON.stringify(payload) }); },
  remove(endpoint, id) { return this.request(`${endpoint}/${id}`, { method: "DELETE" }); },
  login(payload) { return this.request("Auth/login", { method: "POST", body: JSON.stringify(payload) }); },
  register(payload) { return this.request("Auth/register", { method: "POST", body: JSON.stringify(payload) }); },
  refresh(payload) { return this.request("Auth/refresh", { method: "POST", body: JSON.stringify(payload) }); }
};
