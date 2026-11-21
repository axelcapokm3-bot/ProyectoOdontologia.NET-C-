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
    localStorage.removeItem(APP_CONFIG.userKey);
  },

  async request(endpoint, options = {}, _esReintento = false) {
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
      if (configuredUrl === APP_CONFIG.apiBaseUrl) {
        throw new Error(`No se pudo conectar con la API en ${APP_CONFIG.apiBaseUrl}.`);
      }

      try {
        response = await request(APP_CONFIG.apiBaseUrl);
        localStorage.removeItem(APP_CONFIG.storageKey);
      } catch (_) {
        throw new Error(`No se pudo conectar con la API en ${APP_CONFIG.apiBaseUrl}.`);
      }
    }

    if (!response.ok) {
      const endpointNormalizado = endpoint.toLowerCase();

      if (response.status === 401 && endpointNormalizado !== "auth/login" && endpointNormalizado !== "auth/refresh") {
        if (!_esReintento && await this._refreshSession()) {
          return this.request(endpoint, options, true);
        }

        this.clearSession();
        window.location.href = "Login.html";
        throw new Error("Sesión expirada o no autorizada. Por favor, inicia sesión.");
      }

      if (response.status === 403) {
        throw new Error("Tu rol no tiene permisos para realizar esta acción.");
      }

      let message = `La API respondió con el código ${response.status}.`;
      try {
        const problema = await response.json();
        message = problema.detail || problema.title || problema.message || problema.errors?.[Object.keys(problema.errors)[0]]?.[0] || message;
      } catch (_) { }
      throw new Error(message);
    }

    if (response.status === 204) return null;

    // Algunos POST del backend responden 201 sin cuerpo.
    const raw = await response.text();
    if (!raw) return null;

    const contentType = response.headers.get("content-type") || "";
    if (contentType.includes("json")) {
      try { return JSON.parse(raw); } catch (_) { return raw; }
    }
    return raw;
  },

  _refreshSession() {
    if (!this._refreshPromise) {
      this._refreshPromise = this._executeRefresh().finally(() => {
        this._refreshPromise = null;
      });
    }
    return this._refreshPromise;
  },

  async _executeRefresh() {
    const refreshToken = localStorage.getItem(APP_CONFIG.refreshTokenKey);
    if (!refreshToken) return false;

    try {
      const auth = await this.request("Auth/refresh", {
        method: "POST",
        body: JSON.stringify({ refreshToken })
      });
      if (!auth?.accessToken) return false;
      this.setSession(auth);
      return true;
    } catch (_) {
      return false;
    }
  },

  list(endpoint) { return this.request(endpoint); },
  search(endpoint, texto) { return this.request(`${endpoint}/buscar?texto=${encodeURIComponent(texto)}`); },
  create(endpoint, payload) { return this.request(endpoint, { method: "POST", body: JSON.stringify(payload) }); },
  update(endpoint, id, payload) { return this.request(`${endpoint}/${id}`, { method: "PUT", body: JSON.stringify(payload) }); },
  patch(endpoint, id, action) { return this.request(`${endpoint}/${id}/${action}`, { method: "PATCH" }); },
  remove(endpoint, id) { return this.request(`${endpoint}/${id}`, { method: "DELETE" }); },
  login(payload) { return this.request("Auth/login", { method: "POST", body: JSON.stringify(payload) }); },
  register(payload) { return this.request("Auth/register", { method: "POST", body: JSON.stringify(payload) }); },
  registroPaciente(payload) { return this.request("Auth/registro", { method: "POST", body: JSON.stringify(payload) }); },
  refresh(payload) { return this.request("Auth/refresh", { method: "POST", body: JSON.stringify(payload) }); }
};
