/* ============================================================
   OdontoCare — Lógica de presentación
   Consume únicamente los endpoints y reglas de negocio del
   backend (config.js + api.js). No hay datos simulados.
   ============================================================ */

const PAGE_SIZE = 8;

const entities = {
  pacientes: {
    title: "Pacientes",
    singular: "paciente",
    endpoint: "Paciente",
    description: "Completá los datos básicos del paciente.",
    fields: [
      { name: "nombre", label: "Nombre", type: "text", required: true, pattern: "[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+" },
      { name: "apellido", label: "Apellido", type: "text", required: true, pattern: "[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+" },
      { name: "email", label: "Correo electrónico", type: "email", required: true },
      { name: "fechaNacimiento", label: "Fecha de nacimiento", type: "date", required: true, min: "1920-01-01", max: "2007-12-31" },
      { name: "telefono", label: "Teléfono", type: "tel", required: true }
    ],
    row: (p) => `
      <td class="font-mono">#${p.id}</td>
      <td class="font-semibold"><div class="user-cell"><span class="avatar-text">${esc(iniciales(`${p.nombre} ${p.apellido}`))}</span><span>${esc(p.nombre)} ${esc(p.apellido)}</span></div></td>
      <td class="text-muted hide-sm">${esc(p.email)}</td>
      <td class="hide-md">${esc(p.telefono)}</td>
      <td class="hide-md">${esc(formatDate(p.fechaNacimiento))}</td>`
  },
  odontologos: {
    title: "Odontólogos",
    singular: "odontólogo",
    endpoint: "Odontologo",
    description: "Registrá los datos profesionales y de contacto.",
    fields: [
      { name: "nombre", label: "Nombre completo", type: "text", required: true },
      { name: "matricula", label: "Matrícula", type: "text", required: true },
      { name: "especialidad", label: "Especialidad", type: "text", required: true },
      { name: "telefono", label: "Teléfono", type: "number", min: "10000000", max: "2147483647", step: "1", required: true }
    ],
    row: (o) => `
      <td class="font-mono">#${o.id}</td>
      <td class="font-semibold"><div class="user-cell"><span class="avatar-text">${esc(iniciales(o.nombre))}</span><span>${esc(o.nombre)}</span></div></td>
      <td class="hide-sm">${esc(o.matricula)}</td>
      <td><span class="tag">${esc(o.especialidad)}</span></td>
      <td class="hide-md">${esc(o.telefono)}</td>`
  },
  tratamientos: {
    title: "Tratamientos",
    singular: "tratamiento",
    endpoint: "Tratamiento",
    description: "Definí los procedimientos y los insumos que consumen.",
    fields: [
      { name: "descripcion", label: "Descripción", type: "text", required: true },
      { name: "costo", label: "Costo", type: "number", min: "0", step: "0.01", required: true },
      { name: "duracionMinutos", label: "Duración (minutos)", type: "number", min: "1", step: "1", required: true, defaultValue: 30 }
    ],
    row: (t) => `
      <td class="font-mono">TRT-${String(t.id).padStart(3, "0")}</td>
      <td class="font-semibold">${esc(t.descripcion)}<span class="row-sub">${esc(t.duracionMinutos ?? 30)} min</span></td>
      <td class="hide-md">${insumosRequeridosChips(t)}</td>
      <td class="text-right font-medium">${formatCurrency(t.costo)}</td>`
  },
  turnos: {
    title: "Turnos",
    singular: "turno",
    endpoint: "Turnos",
    description: "Asociá el turno a un paciente, profesional y tratamiento.",
    fields: [
      { name: "fechaHoraInicio", label: "Fecha y hora de inicio", type: "datetime-local", required: true },
      { name: "pacienteId", label: "Paciente", type: "select", source: "pacientes", required: true },
      { name: "odontologoId", label: "Odontólogo", type: "select", source: "odontologos", required: true },
      { name: "tratamientoId", label: "Tratamiento", type: "select", source: "tratamientos", required: true }
    ],
    row: (t) => `
      <td><span class="cell-strong text-primary">${esc(formatDateTime(t.fechaHoraInicio))}</span><span class="row-sub">Fin: ${esc(formatDateTime(t.fechaHoraFin))}</span></td>
      <td class="font-semibold">${esc(nameById("pacientes", t.pacienteId))}</td>
      <td class="text-muted">${esc(nameById("odontologos", t.odontologoId))}</td>
      <td class="text-muted hide-md">${esc(nameById("tratamientos", t.tratamientoId))}</td>
      <td class="text-center">${badgeHtml(estadoTurno(t).text, estadoTurno(t).type)}</td>`
  },
  insumos: {
    title: "Insumos",
    singular: "insumo",
    endpoint: "Insumos",
    description: "El punto de pedido se calcula automáticamente según la categoría.",
    fields: [
      { name: "nombre", label: "Nombre del insumo", type: "text", required: true, pattern: "[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+" },
      { name: "categoria", label: "Categoría", type: "select", required: true, options: [["1", "Descartable"], ["2", "Anestesia"], ["3", "Restauración"], ["4", "Instrumental"]] },
      { name: "stock", label: "Stock inicial", type: "number", min: "0", step: "1", required: true }
    ],
    row: (i) => {
      const estado = estadoInsumo(i);
      const disponible = stockDisponible(i);
      const punto = Math.max(1, Number(i.puntoPedido) || 0);
      const porcentaje = Math.max(4, Math.min(100, Math.round((disponible / punto) * 100)));
      const tono = estado.type === "critico" ? "critical" : estado.type === "alerta" ? "mid" : "";
      return `
      <td class="font-mono">#INV-${String(i.id).padStart(3, "0")}</td>
      <td class="font-semibold">${esc(i.nombre)}</td>
      <td class="hide-sm">${esc(categoriaDeInsumo(i))}</td>
      <td class="text-right">
        <div class="stock-cell">
          <span class="font-semibold ${estado.type === "critico" ? "text-danger" : ""}">${disponible} uds</span>
          <span class="stock-meter" aria-hidden="true"><span class="stock-meter-fill ${tono ? `stock-meter-fill--${tono}` : ""}" style="width:${porcentaje}%"></span></span>
        </div>
      </td>
      <td class="text-right hide-md text-muted">${esc(i.puntoPedido)} uds</td>
      <td class="text-center">${badgeHtml(estado.text, estado.type)}</td>`;
    }
  }
};

/* ---------- Estado global ---------- */
const state = {
  data: { pacientes: [], odontologos: [], tratamientos: [], turnos: [], insumos: [] },
  search: { pacientes: null, odontologos: null, tratamientos: null, turnos: null, insumos: null },
  page: { pacientes: 1, odontologos: 1, tratamientos: 1, turnos: 1, insumos: 1 },
  editing: null,
  tiList: []
};

const $ = (selector) => document.querySelector(selector);
const pad = (n) => String(n).padStart(2, "0");
const debounce = (fn, wait) => { let t; return (...args) => { clearTimeout(t); t = setTimeout(() => fn(...args), wait); }; };

const ROLE_LABELS = { Admin: "Administrador", Odontologo: "Odontólogo", Recepcionista: "Recepcionista", Paciente: "Paciente" };

function readSession() {
  try {
    return JSON.parse(localStorage.getItem(APP_CONFIG.userKey)) || null;
  } catch (_) {
    return null;
  }
}

const session = readSession();
const rol = session?.rol || "";
const rolesConPanel = new Set(["Admin", "Odontologo", "Recepcionista"]);
const tieneAccesoPanel = rolesConPanel.has(rol);
const esRecepcionista = rol === "Recepcionista";
const puede = {
  odontologos: !esRecepcionista,
  insumos: !esRecepcionista,
  completarTurnos: !esRecepcionista
};

/* ---------- Helpers de formato ---------- */
function esc(value) {
  return String(value ?? "").replace(/[&<>'"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#039;", '"': "&quot;" })[c]);
}
function formatCurrency(value) {
  return new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 2 }).format(value || 0);
}
function formatDate(value) {
  if (!value) return "—";
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? "—" : new Intl.DateTimeFormat("es-AR", { dateStyle: "medium" }).format(d);
}
function formatDateTime(value) {
  if (!value) return "—";
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? "—" : new Intl.DateTimeFormat("es-AR", { dateStyle: "medium", timeStyle: "short" }).format(d);
}
function formatTodayLong(date) {
  const text = new Intl.DateTimeFormat("es-AR", { weekday: "long", day: "numeric", month: "long" }).format(date);
  return text.charAt(0).toUpperCase() + text.slice(1);
}
function rolLegible(value) {
  return ROLE_LABELS[value] || "Personal clínico";
}
function localDateKey(d) { return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`; }
function localDateTimeNow() { const d = new Date(); return `${localDateKey(d)}T${pad(d.getHours())}:${pad(d.getMinutes())}`; }
function addMinutesToLocalInput(value, minutes) {
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return "";
  d.setMinutes(d.getMinutes() + Number(minutes || 0));
  return `${localDateKey(d)}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
function iniciales(nombre) {
  return String(nombre || "").split(/\s+/).filter(Boolean).slice(0, 2).map((p) => p[0].toUpperCase()).join("");
}
function referenceLabel(source, item) {
  if (source === "pacientes") return `${item.nombre} ${item.apellido}`;
  if (source === "tratamientos") return item.descripcion;
  return item.nombre;
}
function nameById(entity, id) {
  const item = state.data[entity].find((x) => x.id === id);
  if (item) return referenceLabel(entity, item);
  if (entity === "odontologos") return `Odontólogo #${id}`;
  if (entity === "pacientes") return `Paciente #${id}`;
  if (entity === "tratamientos") return `Tratamiento #${id}`;
  return `#${id}`;
}

/* ---------- Reglas de negocio del backend (derivadas) ---------- */
// CategoriaInsumo: Descartable=1, Anestesia=2, Restauracion=3, Instrumental=4
// PuntoPedido = StockSeguridad * 2, donde StockSeguridad depende de la categoría.
const CATEGORIA_POR_PUNTO = { 400: "Descartable", 100: "Anestesia", 40: "Restauración", 20: "Instrumental" };
const CATEGORIA_ID_POR_PUNTO = { 400: "1", 100: "2", 40: "3", 20: "4" };

function stockDisponible(item) { return (item.stock || 0) - (item.stockReservado || 0); }
function categoriaDeInsumo(item) { return CATEGORIA_POR_PUNTO[item.puntoPedido] || "General"; }

// NivelCriticidad del backend: Critico si disponible <= StockSeguridad;
// Medio si disponible < PuntoPedido; si no Bajo.
function estadoInsumo(item) {
  const disponible = stockDisponible(item);
  const punto = item.puntoPedido || 0;
  const seguridad = Math.round(punto / 2);
  if (disponible <= seguridad) return { text: "Crítico", type: "critico" };
  if (disponible < punto) return { text: "Medio", type: "alerta" };
  return { text: "OK", type: "ok" };
}

function badgeHtml(text, type) { return `<span class="badge badge-${type}">${esc(text)}</span>`; }
function estadoTurno(item) {
  if (item.estado === 2 || item.estado === "Completado") return { text: "Completado", type: "ok" };
  if (item.estado === 3 || item.estado === "Cancelado") return { text: "Cancelado", type: "critico" };
  return { text: "Pendiente", type: "alerta" };
}
function actionCell(entity, item) {
  const id = item.id;
  const pendiente = estadoTurno(item).text === "Pendiente";
  const esTurno = entity === "turnos";
  const puedeEditar = !esTurno || pendiente;
  const tituloEditar = puedeEditar ? "Editar" : "Solo se pueden editar turnos pendientes";
  const completo = puede.completarTurnos
    ? `<button class="icon-button" title="Completar" aria-label="Completar turno" data-action="complete" data-entity="${entity}" data-id="${id}" ${pendiente ? "" : "disabled"}><span class="material-symbols-outlined" aria-hidden="true">check_circle</span></button>`
    : "";
  const accionesTurno = esTurno
    ? `${completo}
       <button class="icon-button text-danger" title="Cancelar" aria-label="Cancelar turno" data-action="cancel" data-entity="${entity}" data-id="${id}" ${pendiente ? "" : "disabled"}><span class="material-symbols-outlined" aria-hidden="true">cancel</span></button>`
    : "";
  return `<td class="text-right"><div class="action-buttons">
    ${accionesTurno}
    <button class="icon-button" title="${tituloEditar}" aria-label="${tituloEditar}" data-action="edit" data-entity="${entity}" data-id="${id}" ${puedeEditar ? "" : "disabled"}><span class="material-symbols-outlined" aria-hidden="true">edit</span></button>
    <button class="icon-button text-danger" title="Eliminar" aria-label="Eliminar" data-action="delete" data-entity="${entity}" data-id="${id}"><span class="material-symbols-outlined" aria-hidden="true">delete</span></button>
  </div></td>`;
}

function insumosRequeridosChips(t) {
  if (!t.tratamientoInsumo || !t.tratamientoInsumo.length) return `<span class="text-muted">Sin insumos</span>`;
  return t.tratamientoInsumo.map((rel) => {
    const insumo = state.data.insumos.find((i) => i.id === rel.insumoId);
    return `<span class="tag">${esc(insumo ? insumo.nombre : `#${rel.insumoId}`)} × ${rel.cantidadUsada}</span>`;
  }).join(" ");
}

/* ---------- Navegación de secciones ---------- */
const SECCIONES = ["dashboard", "pacientes", "odontologos", "tratamientos", "turnos", "insumos"];
const seccionesDisponibles = new Set(SECCIONES.filter((name) => {
  if (name === "odontologos" && !puede.odontologos) return false;
  if (name === "insumos" && !puede.insumos) return false;
  return true;
}));

function mostrarSeccion(name, actualizarHash = true) {
  const destino = seccionesDisponibles.has(name) ? name : "dashboard";
  document.querySelectorAll(".seccion-modulo").forEach((sec) => sec.classList.add("hidden"));
  const target = document.getElementById(`sec-${destino}`);
  if (target) target.classList.remove("hidden");
  document.querySelectorAll("#entityNav .nav-link").forEach((b) => {
    const activo = b.dataset.seccion === destino;
    b.classList.toggle("active", activo);
    if (activo) b.setAttribute("aria-current", "page");
    else b.removeAttribute("aria-current");
  });
  if (actualizarHash) history.replaceState(null, "", `#${destino}`);
}

function seccionDesdeHash() {
  return (window.location.hash || "").replace("#", "");
}

function aplicarPermisos() {
  if (esRecepcionista) document.body.classList.add("rol-recepcion");
  if (!puede.odontologos) {
    document.querySelector('[data-seccion="odontologos"]')?.remove();
    document.getElementById("sec-odontologos")?.remove();
    document.querySelector('label[for="filter-doctor"]')?.closest(".filter-group")?.remove();
  }
  if (!puede.insumos) {
    document.querySelector('[data-seccion="insumos"]')?.remove();
    document.getElementById("sec-insumos")?.remove();
    document.querySelector(".metric-card--stock")?.remove();
    document.getElementById("stockPanel")?.remove();
  }
}

function inicializarSesion() {
  if (!session) return;
  const nombre = session.nombreUsuario || "Sesión activa";
  const avatar = document.getElementById("userAvatar");
  const nombreEl = document.getElementById("userName");
  const rolEl = document.getElementById("userRole");
  if (avatar) avatar.textContent = iniciales(nombre) || "OC";
  if (nombreEl) nombreEl.textContent = nombre;
  if (rolEl) rolEl.textContent = rolLegible(session.rol);
}

/* ---------- Estados de carga y vacío ---------- */
function setBusy(busy) {
  document.body.classList.toggle("is-busy", busy);
}

function renderSkeleton(entity, rows = 6) {
  const tbody = $(`#tbody-${entity}`);
  if (!tbody) return;
  const cols = document.querySelectorAll(`#sec-${entity} thead th`).length || 6;
  const pieces = ["skeleton-line-sm", "skeleton-line-lg", "skeleton-line", "skeleton-line-sm", "skeleton-line", "skeleton-line-sm", "skeleton-line"];
  tbody.innerHTML = Array.from({ length: rows }, () => `
    <tr class="skeleton-row">
      ${Array.from({ length: cols }, (_, index) => `<td><span class="skeleton ${pieces[index % pieces.length]}"></span></td>`).join("")}
    </tr>`).join("");
}

function renderDashboardSkeleton(rows = 5) {
  const tbody = $("#tbody-proximas-citas");
  if (!tbody) return;
  tbody.innerHTML = Array.from({ length: rows }, () => `
    <tr class="skeleton-row">
      <td><span class="skeleton skeleton-line"></span></td>
      <td><span class="skeleton skeleton-line-sm"></span></td>
      <td><span class="skeleton skeleton-line-lg"></span></td>
      <td><span class="skeleton skeleton-line"></span></td>
      <td><span class="skeleton skeleton-line-sm"></span></td>
    </tr>`).join("");
}

function emptyStateHtml({ icon, title, text, action = "" }) {
  return `<tr><td colspan="20" class="loading-cell">
    <div class="empty-state">
      <span class="empty-icon"><span class="material-symbols-outlined" aria-hidden="true">${icon}</span></span>
      <p class="empty-title">${esc(title)}</p>
      <p class="empty-text">${esc(text)}</p>
      ${action ? `<div class="empty-actions">${action}</div>` : ""}
    </div>
  </td></tr>`;
}

function emptyTableHtml(entity, filtered) {
  const cfg = entities[entity];
  if (filtered) {
    return emptyStateHtml({
      icon: "search_off",
      title: "Sin resultados",
      text: "Probá con otro término de búsqueda o quitá los filtros activos.",
      action: `<button class="button button-ghost button-sm" type="button" data-clear-filters="${entity}">Limpiar filtros</button>`
    });
  }
  return emptyStateHtml({
    icon: "inbox",
    title: "Todavía no hay registros",
    text: `Creá el primer ${cfg.singular} para empezar a trabajar con la agenda.`,
    action: `<button class="button button-primary button-sm" type="button" data-new-entity="${entity}">
      <span class="material-symbols-outlined" aria-hidden="true">add</span>
      <span>Crear ${esc(cfg.singular)}</span>
    </button>`
  });
}

function limpiarFiltros(entity) {
  const searchInput = $(`#search-${entity}`);
  if (searchInput) searchInput.value = "";
  state.search[entity] = null;
  state.page[entity] = 1;
  if (entity === "turnos") {
    const date = $("#filter-date");
    const doctor = $("#filter-doctor");
    if (date) date.value = "";
    if (doctor) doctor.value = "all";
  }
  renderTable(entity);
}

/* ---------- Carga de datos ---------- */
async function loadEntity(entity) {
  try {
    state.data[entity] = await Api.list(entities[entity].endpoint);
    state.page[entity] = 1;
    setStatus(true);
  } catch (error) {
    state.data[entity] = [];
    setStatus(false);
  }
  renderAll();
  const term = ($(`#search-${entity}`)?.value || "").trim();
  if (term) searchEntity(entity, term);
}

// Busqueda hibrida del backend: GET /buscar?texto= (por ID o por texto)
async function searchEntity(entity, rawTerm) {
  const term = (rawTerm || "").trim();
  try {
    state.search[entity] = term
      ? await Api.search(entities[entity].endpoint, term)
      : null;
  } catch (error) {
    state.search[entity] = null;
    toast(error.message, "error");
  }
  state.page[entity] = 1;
  renderTable(entity);
}

async function loadAll() {
  const keys = Object.keys(entities).filter((key) => {
    if (key === "odontologos" && !puede.odontologos) return false;
    if (key === "insumos" && !puede.insumos) return false;
    return true;
  });

  keys.forEach((key) => renderSkeleton(key));
  renderDashboardSkeleton();
  setBusy(true);

  const results = await Promise.allSettled(keys.map((k) => Api.list(entities[k].endpoint)));
  results.forEach((res, i) => { state.data[keys[i]] = res.status === "fulfilled" ? res.value : []; });
  setStatus(results.every((r) => r.status === "fulfilled"));
  populateDoctorFilter();
  renderAll();
  setBusy(false);

  if (!results.every((r) => r.status === "fulfilled")) {
    toast("No se pudo cargar toda la información. Revisá la conexión con la API.", "error");
  }
}

function populateDoctorFilter() {
  const select = $("#filter-doctor");
  if (!select) return;
  const actual = select.value;
  select.innerHTML = `<option value="all">Todos los profesionales</option>` +
    state.data.odontologos.map((o) => `<option value="${o.id}">${esc(o.nombre)}</option>`).join("");
  if (Array.from(select.options).some((opt) => opt.value === actual)) select.value = actual;
}

/* ---------- Renderizado de tablas ---------- */
function filteredRecords(entity) {
  let list = state.search[entity] || state.data[entity] || [];
  if (entity === "turnos") list = applyTurnoFilters(list);
  return list;
}

function applyTurnoFilters(list) {
  const date = $("#filter-date")?.value;
  const doctor = $("#filter-doctor")?.value;
  return list.filter((t) => {
    const d = new Date(t.fechaHoraInicio);
    if (date && localDateKey(d) !== date) return false;
    if (doctor && doctor !== "all" && String(t.odontologoId) !== doctor) return false;
    return true;
  });
}

function renderTable(entity) {
  const tbody = $(`#tbody-${entity}`);
  if (!tbody) return;
  const filtered = filteredRecords(entity);
  const total = filtered.length;
  const pages = Math.max(1, Math.ceil(total / PAGE_SIZE));
  if (state.page[entity] > pages) state.page[entity] = pages;
  const start = (state.page[entity] - 1) * PAGE_SIZE;
  const pageItems = filtered.slice(start, start + PAGE_SIZE);

  if (!pageItems.length) {
    tbody.innerHTML = emptyTableHtml(entity, total > 0);
  } else {
    tbody.innerHTML = pageItems.map((item) => `<tr>${entities[entity].row(item)}${actionCell(entity, item)}</tr>`).join("");
  }

  const summary = $(`#summary-${entity}`);
  if (summary) summary.textContent = `${total} ${total === 1 ? "registro" : "registros"}`;
  const pageInfo = $(`#page-${entity}`);
  if (pageInfo) pageInfo.textContent = `${state.page[entity]} / ${pages}`;
  const prev = $(`[data-nav="${entity}:prev"]`);
  const next = $(`[data-nav="${entity}:next"]`);
  if (prev) prev.disabled = state.page[entity] <= 1;
  if (next) next.disabled = state.page[entity] >= pages;
}

function renderAll() {
  Object.keys(entities).forEach(renderTable);
  renderDashboard();
}

/* ---------- Dashboard ---------- */
function renderDistribution(barId, legendId, totalId, segments, emptyText, unit = "registro") {
  const bar = $(barId);
  const legend = $(legendId);
  const totalEl = $(totalId);
  if (!bar || !legend) return;

  const total = segments.reduce((sum, segment) => sum + segment.value, 0);
  const unitLabel = total === 1 ? unit : `${unit}s`;
  if (totalEl) totalEl.textContent = `${total} ${unitLabel}`;

  if (!total) {
    bar.innerHTML = "";
    legend.innerHTML = `<span class="panel-empty">${esc(emptyText)}</span>`;
    return;
  }

  bar.innerHTML = segments
    .filter((segment) => segment.value > 0)
    .map((segment) => `<span class="stacked-segment stacked-segment--${segment.tone}" style="width:${((segment.value / total) * 100).toFixed(2)}%" title="${esc(segment.label)}: ${segment.value}"></span>`)
    .join("");

  legend.innerHTML = segments
    .map((segment) => `<span class="legend-item"><span class="legend-dot legend-dot--${segment.tone}"></span>${esc(segment.label)} <span class="legend-value">${segment.value}</span></span>`)
    .join("");
}

function renderDashboard() {
  const ahora = new Date();
  const today = localDateKey(ahora);
  const month = today.slice(0, 7);

  const setText = (id, value) => { const el = $(id); if (el) el.textContent = value; };
  const turnosHoy = state.data.turnos.filter((t) => localDateKey(new Date(t.fechaHoraInicio)) === today);
  const pendientesHoy = turnosHoy.filter((t) => estadoTurno(t).text === "Pendiente");
  const completadosHoy = turnosHoy.filter((t) => estadoTurno(t).text === "Completado");
  const canceladosHoy = turnosHoy.filter((t) => estadoTurno(t).text === "Cancelado");
  const criticos = state.data.insumos.filter((i) => estadoInsumo(i).type === "critico");
  const stockMedio = state.data.insumos.filter((i) => estadoInsumo(i).type === "alerta");
  const insumosOk = state.data.insumos.filter((i) => estadoInsumo(i).type === "ok");

  const ingresos = state.data.turnos
    .filter((t) => {
      const esDelMes = localDateKey(new Date(t.fechaHoraInicio)).slice(0, 7) === month;
      return esDelMes && estadoTurno(t).text !== "Cancelado";
    })
    .reduce((sum, t) => {
      const tr = state.data.tratamientos.find((x) => x.id === t.tratamientoId);
      return sum + (tr ? Number(tr.costo) : 0);
    }, 0);

  setText("#todayLabel", formatTodayLong(ahora));
  setText("#metric-pacientes", state.data.pacientes.length.toLocaleString("es-AR"));
  setText("#metric-pacientes-sub", "Registros en el sistema");
  setText("#metric-citas-hoy", pendientesHoy.length);
  setText("#metric-citas-sub", `${completadosHoy.length} completados · ${canceladosHoy.length} cancelados`);
  setText("#metric-insumos", criticos.length);
  setText("#metric-insumos-sub", `${stockMedio.length} con stock medio`);
  setText("#metric-ingresos", formatCurrency(ingresos));
  setText("#metric-ingresos-sub", "Turnos no cancelados del mes");

  const alerta = $("#insumosAlert");
  if (alerta) {
    alerta.textContent = criticos.length;
    alerta.hidden = criticos.length === 0;
  }

  renderDistribution(
    "#agenda-bar",
    "#agenda-legend",
    "#agenda-total",
    [
      { label: "Pendientes", value: pendientesHoy.length, tone: "pending" },
      { label: "Completados", value: completadosHoy.length, tone: "done" },
      { label: "Cancelados", value: canceladosHoy.length, tone: "cancelled" }
    ],
    "No hay turnos registrados para hoy.",
    "turno"
  );

  renderDistribution(
    "#stock-bar",
    "#stock-legend",
    "#stock-total",
    [
      { label: "OK", value: insumosOk.length, tone: "ok" },
      { label: "Stock medio", value: stockMedio.length, tone: "mid" },
      { label: "Críticos", value: criticos.length, tone: "critical" }
    ],
    "No hay insumos registrados.",
    "insumo"
  );

  const proximos = state.data.turnos
    .filter((t) => new Date(t.fechaHoraInicio) >= ahora && estadoTurno(t).text === "Pendiente")
    .sort((a, b) => new Date(a.fechaHoraInicio) - new Date(b.fechaHoraInicio))
    .slice(0, 6);

  const tbody = $("#tbody-proximas-citas");
  if (!tbody) return;
  if (!proximos.length) {
    tbody.innerHTML = emptyStateHtml({
      icon: "event_busy",
      title: "Sin turnos próximos",
      text: "No hay turnos pendientes en agenda. Creá uno para comenzar.",
      action: `<button class="button button-primary button-sm" type="button" data-new-entity="turnos">
        <span class="material-symbols-outlined" aria-hidden="true">add</span>
        <span>Nuevo turno</span>
      </button>`
    });
    return;
  }
  tbody.innerHTML = proximos.map((t) => `
    <tr>
      <td class="font-semibold">${esc(nameById("pacientes", t.pacienteId))}</td>
      <td class="text-muted">${esc(nameById("odontologos", t.odontologoId))}</td>
      <td class="text-muted hide-md">${esc(nameById("tratamientos", t.tratamientoId))}</td>
      <td class="text-muted">${esc(formatDateTime(t.fechaHoraInicio))}</td>
      <td class="text-center">${badgeHtml(estadoTurno(t).text, estadoTurno(t).type)}</td>
    </tr>`).join("");
}

/* ---------- Modal CRUD ---------- */
function openModal(entity, id = null) {
  const record = id ? state.data[entity].find((r) => r.id === id) || null : null;
  state.editing = { entity, id: record ? record.id : null };
  state.tiList = [];

  const cfg = entities[entity];
  $("#formTitle").textContent = record ? `Actualizar ${cfg.singular}` : `Registrar ${cfg.singular}`;
  $("#formDescription").textContent = record
    ? `${cfg.description} · Registro #${record.id}`
    : cfg.description;
  const submitLabel = $("#submitButtonLabel");
  if (submitLabel) submitLabel.textContent = record ? "Guardar cambios" : "Guardar";

  renderFormFields(entity, record);

  if (entity === "tratamientos") {
    state.tiList = (record?.tratamientoInsumo || []).map((rel) => {
      const insumo = state.data.insumos.find((i) => i.id === rel.insumoId);
      return { id: rel.insumoId, nombre: insumo ? insumo.nombre : `#${rel.insumoId}`, cantidad: rel.cantidadUsada };
    });
    $("#tiInsumo").innerHTML = `<option value="">Seleccionar insumo…</option>` +
      state.data.insumos.map((i) => `<option value="${i.id}">${esc(i.nombre)}</option>`).join("");
    renderTiList();
    $("#tiEditor").classList.remove("hidden");
  } else {
    $("#tiEditor").classList.add("hidden");
  }

  const fechaHoraInicio = $("#field-fechaHoraInicio");
  if (fechaHoraInicio && !state.editing.id) fechaHoraInicio.min = localDateTimeNow();

  if (entity === "insumos" && record) {
    const cat = CATEGORIA_ID_POR_PUNTO[record.puntoPedido];
    const sel = $("#field-categoria");
    if (sel && cat) sel.value = cat;
  }

  $("#recordDialog").showModal();
  requestAnimationFrame(() => {
    const first = $("#recordDialog").querySelector("input, select");
    if (first) first.focus();
  });
}

function renderFormFields(entity, record) {
  const cfg = entities[entity];
  $("#formFields").innerHTML = cfg.fields.map((field) => {
    const value = record?.[field.name] ?? field.defaultValue;
    const required = field.required ? "required" : "";
    const min = field.min ? ` min="${field.min}"` : "";
    const max = field.max ? ` max="${field.max}"` : "";
    const step = field.step ? ` step="${field.step}"` : "";
    const pattern = field.pattern ? ` pattern="${field.pattern}"` : "";
    const label = entity === "insumos" && field.name === "stock" && record ? "Stock total" : field.label;
    const wide = field.name === "descripcion" ? " span-2" : "";

    if (field.type === "select") {
      let options = field.options || (state.data[field.source] || []).map((item) => [item.id, referenceLabel(field.source, item)]);
      if (field.source === "odontologos" && !options.length) {
        options = Array.from(new Set(state.data.turnos.map((turno) => turno.odontologoId)))
          .map((doctorId) => [doctorId, `Odontólogo #${doctorId}`]);
      }
      const selected = value == null ? "" : String(value);
      return `<label class="field-label${wide}">${label}
        <select id="field-${field.name}" class="form-select" ${required}>
          <option value="">Seleccionar…</option>
          ${options.map(([id, optionLabel]) => `<option value="${esc(id)}" ${selected === String(id) ? "selected" : ""}>${esc(optionLabel)}</option>`).join("")}
        </select></label>`;
    }

    const normalized = field.type === "date" ? String(value ?? "").slice(0, 10)
      : field.type === "datetime-local" ? String(value ?? "").slice(0, 16)
      : (value ?? "");
    return `<label class="field-label${wide}">${label}
      <input id="field-${field.name}" type="${field.type}" class="form-input" value="${esc(normalized)}" ${required}${min}${max}${step}${pattern} autocomplete="off" /></label>`;
  }).join("");
}

function renderTiList() {
  const editing = Boolean(state.editing && state.editing.id);
  const list = $("#tiList");
  if (!list) return;
  list.innerHTML = state.tiList.length
    ? state.tiList.map((ti) => `
      <li class="ti-item">
        <span class="ti-name">${esc(ti.nombre)}</span>
        <span class="ti-qty">× ${ti.cantidad}</span>
        <button type="button" class="icon-button text-danger" title="Quitar" data-ti-remove="${ti.id}">
          <span class="material-symbols-outlined" style="font-size: 18px">remove_circle</span>
        </button>
      </li>`).join("")
    : `<li class="ti-item"><span class="text-muted">Sin insumos asociados.</span></li>`;
  const hint = $("#tiHint");
  if (hint) hint.classList.toggle("hidden", !(editing && !state.tiList.length));
}

function buildPayload(entity) {
  const payload = {};
  entities[entity].fields.forEach((field) => {
    const el = $(`#field-${field.name}`);
    if (!el) return;
    const value = el.value.trim();
    const numeric = field.type === "number" || field.name.endsWith("Id") || field.name === "categoria";
    payload[field.name] = numeric ? (value === "" ? null : Number(value)) : value;
  });
  if (entity === "tratamientos") {
    payload.tratamientoInsumo = state.tiList.map((ti) => ({ insumoId: ti.id, cantidadUsada: ti.cantidad }));
  }
  if (entity === "turnos" && state.editing?.id) {
    const tratamiento = state.data.tratamientos.find((t) => t.id === payload.tratamientoId);
    const duracionMinutos = Number(tratamiento?.duracionMinutos) || 30;
    payload.fechaHoraFin = addMinutesToLocalInput(payload.fechaHoraInicio, duracionMinutos);
  }
  return payload;
}

/* ---------- Estado / Toast ---------- */
function setStatus(connected) {
  const dot = $("#statusDot");
  const text = $("#statusText");
  if (dot) dot.classList.toggle("online", connected);
  if (text) text.textContent = connected ? "API conectada" : "API sin conexión";
}

function toast(message, type = "success") {
  const el = $("#toast");
  if (!el) return;
  const icon = type === "error" ? "error" : "check_circle";
  el.innerHTML = `<span class="material-symbols-outlined toast-icon" aria-hidden="true">${icon}</span><span class="toast-message"></span>`;
  el.querySelector(".toast-message").textContent = message;
  el.setAttribute("role", type === "error" ? "alert" : "status");
  el.className = `toast show ${type}`;
  clearTimeout(toast._timer);
  toast._timer = setTimeout(() => { el.className = "toast"; }, 4200);
}

/* ---------- Diálogo de confirmación ---------- */
function confirmAction({
  title = "Confirmar acción",
  message = "",
  confirmLabel = "Confirmar",
  tone = "danger",
  icon = "help"
} = {}) {
  const dialog = $("#confirmDialog");
  if (!dialog) return Promise.resolve(window.confirm(message));

  const titleEl = $("#confirmTitle");
  const messageEl = $("#confirmMessage");
  const okButton = $("#confirmOk");
  const cancelButton = $("#confirmCancel");
  const iconBox = $("#confirmIcon");
  const iconEl = iconBox?.querySelector(".material-symbols-outlined");

  if (titleEl) titleEl.textContent = title;
  if (messageEl) messageEl.textContent = message;
  if (okButton) {
    okButton.textContent = confirmLabel;
    okButton.className = `button ${tone === "danger" ? "button-danger" : "button-primary"}`;
  }
  if (iconBox) iconBox.classList.toggle("is-danger", tone === "danger");
  if (iconEl) iconEl.textContent = icon;

  dialog.showModal();

  return new Promise((resolve) => {
    const finalizar = (value) => {
      okButton?.removeEventListener("click", alConfirmar);
      cancelButton?.removeEventListener("click", alCancelar);
      dialog.removeEventListener("cancel", alCancelarTecla);
      dialog.removeEventListener("click", alClicFuera);
      if (dialog.open) dialog.close();
      resolve(value);
    };
    const alConfirmar = () => finalizar(true);
    const alCancelar = () => finalizar(false);
    const alCancelarTecla = (event) => { event.preventDefault(); finalizar(false); };
    const alClicFuera = (event) => { if (event.target === dialog) finalizar(false); };

    okButton?.addEventListener("click", alConfirmar);
    cancelButton?.addEventListener("click", alCancelar);
    dialog.addEventListener("cancel", alCancelarTecla);
    dialog.addEventListener("click", alClicFuera);
    okButton?.focus();
  });
}

/* ---------- Eventos: secciones ---------- */
document.querySelectorAll("[data-seccion]").forEach((btn) => {
  btn.addEventListener("click", () => mostrarSeccion(btn.dataset.seccion));
});

/* ---------- Eventos: búsqueda, filtros y paginación ---------- */
Object.keys(entities).forEach((entity) => {
  const input = $(`#search-${entity}`);
  if (input) input.addEventListener("input", debounce(() => searchEntity(entity, input.value), 350));
});

["#filter-date", "#filter-doctor"].forEach((sel) => {
  const el = $(sel);
  if (el) el.addEventListener("change", () => { state.page.turnos = 1; renderTable("turnos"); });
});

$("#mainContent").addEventListener("click", (event) => {
  const nav = event.target.closest("[data-nav]");
  if (nav) {
    const [entity, dir] = nav.dataset.nav.split(":");
    state.page[entity] += dir === "next" ? 1 : -1;
    renderTable(entity);
  }
});

/* ---------- Eventos: botones de acción (editar / eliminar) ---------- */
document.addEventListener("click", async (event) => {
  const logout = event.target.closest("[data-logout]");
  if (logout) {
    Api.clearSession();
    window.location.replace("Login.html");
    return;
  }
  const openSettings = event.target.closest("[data-open-settings]");
  if (openSettings) {
    mostrarConfiguracion();
    return;
  }
  const newEntity = event.target.closest("[data-new-entity]");
  if (newEntity) {
    openModal(newEntity.dataset.newEntity);
    return;
  }
  const clearFilters = event.target.closest("[data-clear-filters]");
  if (clearFilters) {
    limpiarFiltros(clearFilters.dataset.clearFilters);
    return;
  }
  const tiRemove = event.target.closest("[data-ti-remove]");
  if (tiRemove) {
    state.tiList = state.tiList.filter((ti) => ti.id !== Number(tiRemove.dataset.tiRemove));
    renderTiList();
    return;
  }
  const complete = event.target.closest('[data-action="complete"]');
  if (complete) {
    const confirmado = await confirmAction({
      title: "Completar turno",
      message: "El turno pasará a completado y se descontará el stock de los insumos del tratamiento.",
      confirmLabel: "Completar",
      tone: "primary",
      icon: "check_circle"
    });
    if (!confirmado) return;
    complete.disabled = true;
    try {
      await Api.patch(entities.turnos.endpoint, complete.dataset.id, "completar");
      toast("Turno completado y stock actualizado.");
      await Promise.all([loadEntity("turnos"), loadEntity("insumos")]);
    } catch (error) {
      toast(error.message, "error");
    } finally {
      complete.disabled = false;
    }
    return;
  }
  const cancel = event.target.closest('[data-action="cancel"]');
  if (cancel) {
    const confirmado = await confirmAction({
      title: "Cancelar turno",
      message: "El turno cambiará a cancelado. Esta acción no se puede revertir desde el panel.",
      confirmLabel: "Cancelar turno",
      tone: "danger",
      icon: "event_busy"
    });
    if (!confirmado) return;
    cancel.disabled = true;
    try {
      await Api.patch(entities.turnos.endpoint, cancel.dataset.id, "cancelar");
      toast("Turno cancelado.");
      await Promise.all([loadEntity("turnos"), loadEntity("insumos")]);
    } catch (error) {
      toast(error.message, "error");
    } finally {
      cancel.disabled = false;
    }
    return;
  }
  const del = event.target.closest('[data-action="delete"]');
  if (del) {
    const entity = del.dataset.entity;
    const confirmado = await confirmAction({
      title: `Eliminar ${entities[entity].singular}`,
      message: `¿Querés eliminar este ${entities[entity].singular}? Esta acción no se puede deshacer.`,
      confirmLabel: "Eliminar",
      tone: "danger",
      icon: "delete"
    });
    if (!confirmado) return;
    del.disabled = true;
    try {
      await Api.remove(entities[entity].endpoint, del.dataset.id);
      toast("Registro eliminado.");
      await loadEntity(entity);
    } catch (error) {
      toast(error.message, "error");
    } finally {
      del.disabled = false;
    }
    return;
  }
  const edit = event.target.closest('[data-action="edit"]');
  if (edit) openModal(edit.dataset.entity, Number(edit.dataset.id));
});

/* ---------- Eventos: botones "Nuevo" ---------- */
$("#btnNuevoPaciente").addEventListener("click", () => openModal("pacientes"));
$("#btnNuevoOdontologo").addEventListener("click", () => openModal("odontologos"));
$("#btnNuevoTratamiento").addEventListener("click", () => openModal("tratamientos"));
$("#btnNuevoTurno").addEventListener("click", () => openModal("turnos"));
$("#btnNuevaCita").addEventListener("click", () => openModal("turnos"));
$("#btnNuevoInsumo").addEventListener("click", () => openModal("insumos"));

/* ---------- Eventos: editor de insumos requeridos ---------- */
$("#tiAdd").addEventListener("click", () => {
  const sel = $("#tiInsumo");
  const cantidadEl = $("#tiCantidad");
  const id = Number(sel.value);
  const cantidad = Number(cantidadEl.value);
  if (!id || !cantidad || cantidad < 1) {
    toast("Seleccioná un insumo y una cantidad válida.", "error");
    return;
  }
  const insumo = state.data.insumos.find((i) => i.id === id);
  const existing = state.tiList.find((ti) => ti.id === id);
  if (existing) existing.cantidad = cantidad;
  else state.tiList.push({ id, nombre: insumo ? insumo.nombre : `#${id}`, cantidad });
  sel.value = "";
  cantidadEl.value = "";
  renderTiList();
});

/* ---------- Eventos: modal ---------- */
$("#cancelButton").addEventListener("click", () => $("#recordDialog").close());
$("#modalCloseButton").addEventListener("click", () => $("#recordDialog").close());
$("#recordDialog").addEventListener("click", (event) => {
  if (event.target === $("#recordDialog")) $("#recordDialog").close();
});

$("#recordForm").addEventListener("submit", async (event) => {
  event.preventDefault();
  const { entity, id } = state.editing || {};
  if (!entity) return;
  if (entity === "tratamientos" && id && !state.tiList.length) {
    toast("Un tratamiento editado debe incluir al menos un insumo.", "error");
    return;
  }
  if (!event.currentTarget.reportValidity()) return;

  const submitButton = $("#submitButton");
  submitButton.disabled = true;
  submitButton.classList.add("is-loading");
  submitButton.setAttribute("aria-busy", "true");

  try {
    const payload = buildPayload(entity);
    if (id) await Api.update(entities[entity].endpoint, id, payload);
    else await Api.create(entities[entity].endpoint, payload);
    $("#recordDialog").close();
    toast(id ? "Cambios guardados." : "Registro creado correctamente.");
    await loadEntity(entity);
  } catch (error) {
    toast(error.message, "error");
  } finally {
    submitButton.disabled = false;
    submitButton.classList.remove("is-loading");
    submitButton.removeAttribute("aria-busy");
  }
});

/* ---------- Eventos: configuración API ---------- */
function mostrarConfiguracion() {
  const dialog = $("#settingsDialog");
  if (!dialog) return;
  const input = $("#apiUrl");
  if (input) input.value = Api.baseUrl;
  if (!dialog.open) dialog.showModal();
}

$("#settingsCloseButton").addEventListener("click", () => $("#settingsDialog").close());
$("#settingsCancelButton").addEventListener("click", () => $("#settingsDialog").close());
$("#settingsDialog").addEventListener("click", (event) => {
  if (event.target === $("#settingsDialog")) $("#settingsDialog").close();
});
$("#settingsForm").addEventListener("submit", async (event) => {
  event.preventDefault();
  const input = $("#apiUrl");
  if (!input.reportValidity()) return;
  const url = input.value.trim().replace(/\/$/, "");
  if (!url) return;
  localStorage.setItem(APP_CONFIG.storageKey, url);
  $("#settingsDialog").close();
  toast("Configuración guardada. Reconectando…");
  await loadAll();
});

/* ---------- Navegación por hash ---------- */
window.addEventListener("hashchange", () => {
  const destino = seccionDesdeHash();
  const valida = seccionesDisponibles.has(destino) ? destino : "dashboard";
  mostrarSeccion(valida, false);
  if (destino !== valida) history.replaceState(null, "", `#${valida}`);
});

/* ---------- Inicialización ---------- */
function iniciarPanel() {
  if (!Api.accessToken || !session || !tieneAccesoPanel) {
    Api.clearSession();
    window.location.replace("Login.html");
    return;
  }

  aplicarPermisos();
  inicializarSesion();

  const dateFilter = $("#filter-date");
  if (dateFilter) dateFilter.value = localDateKey(new Date());

  const solicitada = seccionDesdeHash();
  const inicial = seccionesDisponibles.has(solicitada) ? solicitada : "dashboard";
  mostrarSeccion(inicial, false);
  if (solicitada && solicitada !== inicial) history.replaceState(null, "", `#${inicial}`);
  loadAll();
}

iniciarPanel();
