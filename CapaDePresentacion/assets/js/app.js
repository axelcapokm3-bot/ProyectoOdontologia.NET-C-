/* ============================================================
   OdontoClínica — Lógica de presentación
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
      <td class="text-muted font-mono">#${p.id}</td>
      <td class="font-semibold">${esc(p.nombre)} ${esc(p.apellido)}</td>
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
      { name: "telefono", label: "Teléfono", type: "number", min: "10000000", max: "9999999999", step: "1", required: true }
    ],
    row: (o) => `
      <td class="text-muted font-mono">#${o.id}</td>
      <td class="font-semibold"><div class="user-cell"><div class="avatar-text">${esc(iniciales(o.nombre))}</div>${esc(o.nombre)}</div></td>
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
      { name: "costo", label: "Costo", type: "number", min: "0", step: "0.01", required: true }
    ],
    row: (t) => `
      <td class="text-muted font-mono">TRT-${String(t.id).padStart(3, "0")}</td>
      <td class="font-semibold">${esc(t.descripcion)}</td>
      <td class="hide-md">${insumosRequeridosChips(t)}</td>
      <td class="text-right font-medium">${formatCurrency(t.costo)}</td>`
  },
  turnos: {
    title: "Turnos",
    singular: "turno",
    endpoint: "Turnos",
    description: "Asociá el turno a un paciente, profesional y tratamiento.",
    fields: [
      { name: "fechaHora", label: "Fecha y hora", type: "datetime-local", required: true },
      { name: "pacienteId", label: "Paciente", type: "select", source: "pacientes", required: true },
      { name: "odontologoId", label: "Odontólogo", type: "select", source: "odontologos", required: true },
      { name: "tratamientoId", label: "Tratamiento", type: "select", source: "tratamientos", required: true }
    ],
    row: (t) => `
      <td class="font-semibold text-primary">${esc(formatDateTime(t.fechaHora))}</td>
      <td class="font-semibold">${esc(nameById("pacientes", t.pacienteId))}</td>
      <td class="text-muted">${esc(nameById("odontologos", t.odontologoId))}</td>
      <td class="text-muted hide-md">${esc(nameById("tratamientos", t.tratamientoId))}</td>`
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
      return `
      <td class="text-muted font-mono">#INV-${String(i.id).padStart(3, "0")}</td>
      <td class="font-semibold">${esc(i.nombre)}</td>
      <td class="hide-sm">${esc(categoriaDeInsumo(i))}</td>
      <td class="text-right font-medium ${estado.type === "critico" ? "text-danger" : ""}">${stockDisponible(i)} uds</td>
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
function localDateKey(d) { return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`; }
function localDateTimeNow() { const d = new Date(); return `${localDateKey(d)}T${pad(d.getHours())}:${pad(d.getMinutes())}`; }
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
  return item ? referenceLabel(entity, item) : `#${id}`;
}

/* ---------- Reglas de negocio del backend (derivadas) ---------- */
// CategoriaInsumo: Descartable=1, Anestesia=2, Restauracion=3, Instrumental=4
// PuntoPedido = StockSeguridad * 2, donde StockSeguridad depende de la categoría.
const CATEGORIA_POR_PUNTO = { 400: "Descartable", 100: "Anestesia", 40: "Restauración", 20: "Instrumental" };
const CATEGORIA_ID_POR_PUNTO = { 400: "1", 100: "2", 40: "3", 20: "4" };

function stockDisponible(item) { return (item.stock || 0) - (item.stockReserva || 0); }
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
function actionCell(entity, id) {
  return `<td class="text-right"><div class="action-buttons">
    <button class="icon-button" title="Editar" aria-label="Editar" data-action="edit" data-entity="${entity}" data-id="${id}"><span class="material-symbols-outlined" style="font-size: 18px">edit</span></button>
    <button class="icon-button text-danger" title="Eliminar" aria-label="Eliminar" data-action="delete" data-entity="${entity}" data-id="${id}"><span class="material-symbols-outlined" style="font-size: 18px">delete</span></button>
  </div></td>`;
}

function insumosRequeridosChips(t) {
  if (!t.listaInsumo || !t.listaInsumo.length) return `<span class="text-muted">Sin insumos</span>`;
  return t.listaInsumo.map((rel) => {
    const insumo = state.data.insumos.find((i) => i.id === rel.idInsumo);
    return `<span class="tag">${esc(insumo ? insumo.nombre : `#${rel.idInsumo}`)} × ${rel.cantidad}</span>`;
  }).join(" ");
}

/* ---------- Navegación de secciones ---------- */
function mostrarSeccion(name) {
  document.querySelectorAll(".seccion-modulo").forEach((sec) => sec.classList.add("hidden"));
  const target = document.getElementById(`sec-${name}`);
  if (target) target.classList.remove("hidden");
  document.querySelectorAll("#entityNav .nav-link").forEach((b) => b.classList.toggle("active", b.dataset.seccion === name));
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
  const keys = Object.keys(entities);
  const results = await Promise.allSettled(keys.map((k) => Api.list(entities[k].endpoint)));
  results.forEach((res, i) => { state.data[keys[i]] = res.status === "fulfilled" ? res.value : []; });
  setStatus(results.every((r) => r.status === "fulfilled"));
  populateDoctorFilter();
  renderAll();
  if (!results.every((r) => r.status === "fulfilled")) {
    toast("No se pudo conectar con la API. Revisá la configuración.", "error");
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
  const list = state.search[entity] || state.data[entity] || [];
  if (entity === "turnos") list = applyTurnoFilters(list);
  return list;
}

function applyTurnoFilters(list) {
  const date = $("#filter-date")?.value;
  const doctor = $("#filter-doctor")?.value;
  return list.filter((t) => {
    const d = new Date(t.fechaHora);
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
    tbody.innerHTML = `<tr><td colspan="20" class="loading-cell">${
      total ? "No hay resultados para el filtro actual." : "No hay registros todavía. Usá el botón para crear el primero."
    }</td></tr>`;
  } else {
    tbody.innerHTML = pageItems.map((item) => `<tr>${entities[entity].row(item)}${actionCell(entity, item.id)}</tr>`).join("");
  }

  const summary = $(`#summary-${entity}`);
  if (summary) summary.textContent = `${total} ${total === 1 ? "registro" : "registros"}`;
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
function renderDashboard() {
  const today = localDateKey(new Date());
  const month = today.slice(0, 7);

  const pacientesCount = state.data.pacientes.length;
  const citasHoy = state.data.turnos.filter((t) => localDateKey(new Date(t.fechaHora)) === today);
  const criticos = state.data.insumos.filter((i) => estadoInsumo(i).type !== "ok");
  const ingresos = state.data.turnos
    .filter((t) => localDateKey(new Date(t.fechaHora)).slice(0, 7) === month)
    .reduce((sum, t) => {
      const tr = state.data.tratamientos.find((x) => x.id === t.tratamientoId);
      return sum + (tr ? Number(tr.costo) : 0);
    }, 0);

  const setText = (id, value) => { const el = $(id); if (el) el.textContent = value; };

  setText("#metric-pacientes", pacientesCount.toLocaleString("es-AR"));
  setText("#metric-citas-hoy", citasHoy.length);
  setText("#metric-citas-sub", `${citasHoy.length} ${citasHoy.length === 1 ? "turno" : "turnos"} para hoy`);
  setText("#metric-insumos", criticos.length);
  setText("#metric-ingresos", formatCurrency(ingresos));
  setText("#metric-ingresos-sub", "Estimado del mes por tratamientos");

  const ahora = new Date();
  const proximos = state.data.turnos
    .filter((t) => new Date(t.fechaHora) >= ahora)
    .sort((a, b) => new Date(a.fechaHora) - new Date(b.fechaHora))
    .slice(0, 6);

  const tbody = $("#tbody-proximas-citas");
  if (!tbody) return;
  if (!proximos.length) {
    tbody.innerHTML = `<tr><td colspan="4" class="loading-cell">No hay próximos turnos. Creá un turno para empezar.</td></tr>`;
    return;
  }
  tbody.innerHTML = proximos.map((t) => `
    <tr>
      <td class="font-semibold">${esc(nameById("pacientes", t.pacienteId))}</td>
      <td class="text-muted">${esc(nameById("odontologos", t.odontologoId))}</td>
      <td class="text-muted">${esc(nameById("tratamientos", t.tratamientoId))}</td>
      <td class="text-muted">${esc(formatDateTime(t.fechaHora))}</td>
    </tr>`).join("");
}

/* ---------- Modal CRUD ---------- */
function openModal(entity, id = null) {
  const record = id ? state.data[entity].find((r) => r.id === id) || null : null;
  state.editing = { entity, id: record ? record.id : null };
  state.tiList = [];

  const cfg = entities[entity];
  $("#formTitle").textContent = record ? `Actualizar ${cfg.singular}` : `Registrar ${cfg.singular}`;
  $("#formDescription").textContent = cfg.description;
  $("#submitButton").textContent = record ? "Guardar cambios" : "Guardar";

  renderFormFields(entity, record);

  if (entity === "tratamientos") {
    state.tiList = (record?.listaInsumo || []).map((rel) => {
      const insumo = state.data.insumos.find((i) => i.id === rel.idInsumo);
      return { id: rel.idInsumo, nombre: insumo ? insumo.nombre : `#${rel.idInsumo}`, cantidad: rel.cantidad };
    });
    $("#tiInsumo").innerHTML = `<option value="">Seleccionar insumo…</option>` +
      state.data.insumos.map((i) => `<option value="${i.id}">${esc(i.nombre)}</option>`).join("");
    renderTiList();
    $("#tiEditor").classList.remove("hidden");
  } else {
    $("#tiEditor").classList.add("hidden");
  }

  const fechaHora = $("#field-fechaHora");
  if (fechaHora) fechaHora.min = localDateTimeNow();

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
    const value = record?.[field.name];
    const required = field.required ? "required" : "";
    const min = field.min ? ` min="${field.min}"` : "";
    const max = field.max ? ` max="${field.max}"` : "";
    const pattern = field.pattern ? ` pattern="${field.pattern}"` : "";

    if (field.type === "select") {
      const options = field.options || (state.data[field.source] || []).map((item) => [item.id, referenceLabel(field.source, item)]);
      const selected = value == null ? "" : String(value);
      return `<label class="field-label">${field.label}
        <select id="field-${field.name}" class="form-select" ${required}>
          <option value="">Seleccionar…</option>
          ${options.map(([id, label]) => `<option value="${esc(id)}" ${selected === String(id) ? "selected" : ""}>${esc(label)}</option>`).join("")}
        </select></label>`;
    }

    const normalized = field.type === "date" ? String(value ?? "").slice(0, 10)
      : field.type === "datetime-local" ? String(value ?? "").slice(0, 16)
      : (value ?? "");
    return `<label class="field-label">${field.label}
      <input id="field-${field.name}" type="${field.type}" class="form-input" value="${esc(normalized)}" ${required}${min}${max}${pattern} /></label>`;
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
  el.textContent = message;
  el.className = `toast show ${type}`;
  clearTimeout(toast._timer);
  toast._timer = setTimeout(() => { el.className = "toast"; }, 3600);
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
  const tiRemove = event.target.closest("[data-ti-remove]");
  if (tiRemove) {
    state.tiList = state.tiList.filter((ti) => ti.id !== Number(tiRemove.dataset.tiRemove));
    renderTiList();
    return;
  }
  const del = event.target.closest('[data-action="delete"]');
  if (del) {
    const entity = del.dataset.entity;
    if (!confirm(`¿Querés eliminar este ${entities[entity].singular}? Esta acción no se puede deshacer.`)) return;
    try {
      await Api.remove(entities[entity].endpoint, del.dataset.id);
      toast("Registro eliminado.");
      await loadEntity(entity);
    } catch (error) {
      toast(error.message, "error");
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
  try {
    const payload = buildPayload(entity);
    if (id) await Api.update(entities[entity].endpoint, id, payload);
    else await Api.create(entities[entity].endpoint, payload);
    toast(id ? "Cambios guardados." : "Registro creado correctamente.");
    $("#recordDialog").close();
    await loadEntity(entity);
  } catch (error) {
    toast(error.message, "error");
  }
});

/* ---------- Eventos: configuración API ---------- */
$("#settingsButton").addEventListener("click", () => {
  $("#apiUrl").value = Api.baseUrl;
  $("#settingsDialog").showModal();
});
$("#settingsCloseButton").addEventListener("click", () => $("#settingsDialog").close());
$("#settingsCancelButton").addEventListener("click", () => $("#settingsDialog").close());
$("#settingsDialog").addEventListener("click", (event) => {
  if (event.target === $("#settingsDialog")) $("#settingsDialog").close();
});
$("#settingsForm").addEventListener("submit", (event) => {
  event.preventDefault();
  const url = $("#apiUrl").value.trim().replace(/\/$/, "");
  if (!url) return;
  localStorage.setItem(APP_CONFIG.storageKey, url);
  $("#settingsDialog").close();
  toast("Configuración guardada.");
  loadAll();
});

/* ---------- Inicialización ---------- */
const dateFilter = $("#filter-date");
if (dateFilter) dateFilter.value = localDateKey(new Date());

loadAll();
