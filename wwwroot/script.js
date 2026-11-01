"use strict";
const $ = id => document.getElementById(id);
const today = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,"0")}-${String(d.getDate()).padStart(2,"0")}`; };
let children = [], staff = [], payments = [], activeTab = "dashboard";
const esc = value => String(value ?? "").replace(/[&<>"']/g, ch => ({"&":"&amp;","<":"&lt;",">":"&gt;","\"":"&quot;","'":"&#39;"}[ch]));
const nameOf = id => { const c = children.find(x => x.id === id); return c ? `${c.firstName} ${c.lastName}` : "Unknown child"; };
function notify(message, error = false) { $("notice").textContent = message; $("notice").className = error ? "error" : ""; }
async function api(url, options = {}) {
  const response = await fetch(url, { ...options, headers: { "Content-Type": "application/json", ...(options.headers || {}) } });
  if (!response.ok) { let message = `HTTP ${response.status}`; try { const data = await response.json(); message = data.error || message; } catch {} throw new Error(message); }
  return response.status === 204 ? null : response.json();
}
async function safe(action) { try { await action(); notify("Saved / updated successfully."); } catch (err) { notify(err.message, true); } }
function formData(form) { return Object.fromEntries(new FormData(form).entries()); }
function fillForm(form, data) { for (const [key, value] of Object.entries(data)) { const field = form.elements.namedItem(key); if (field) field.value = value ?? ""; } }
function resetForm(form) { form.reset(); const id = form.elements.namedItem("id"); if (id) id.value = ""; }
function selectOptions() {
  document.querySelectorAll(".child-options").forEach(select => {
    const old = select.value, all = select.dataset.all === "true";
    select.innerHTML = `<option value="">${all ? "All children" : "Select child"}</option>` +
      children.map(c => `<option value="${esc(c.id)}">${esc(c.firstName)} ${esc(c.lastName)}</option>`).join("");
    if ([...select.options].some(x => x.value === old)) select.value = old;
  });
}
async function loadChildren() { children = await api("/api/children"); selectOptions(); renderChildren(); }
function renderChildren() {
  const search = $("child-search").value.trim().toLowerCase();
  $("child-list").innerHTML = children.filter(c => `${c.firstName} ${c.lastName}`.toLowerCase().includes(search)).map(c =>
    `<div class="item"><b>${esc(c.firstName)} ${esc(c.lastName)}</b><small>${esc(c.group)} · ${esc(c.gender)} · Parent: ${esc(c.parentName)} · ${esc(c.phone)}</small>
     <button data-action="edit-child" data-id="${esc(c.id)}">Edit</button><button data-action="delete-child" data-id="${esc(c.id)}">Delete</button></div>`).join("") || "No children found.";
}
async function dashboard() {
  const d = await api(`/api/dashboard?date=${today()}`);
  for (const [id, key] of Object.entries({"d-total":"totalChildren","d-male":"male","d-female":"female","d-present":"present","d-absent":"absent","d-unmarked":"unmarked","d-staff":"staff"})) $(id).textContent = d[key];
  $("d-payments").textContent = Number(d.payments).toLocaleString("en-CA", { style:"currency", currency:"CAD" });
}
async function attendance() {
  const records = await api(`/api/attendance?date=${encodeURIComponent($("attendance-date").value)}`);
  $("attendance-list").innerHTML = children.map(c => {
    const record = records.find(r => r.childId === c.id);
    return `<div class="att-row"><span>${esc(c.firstName)} ${esc(c.lastName)}</span><select data-att-child="${esc(c.id)}"><option value="">Not marked</option><option value="Present" ${record?.status === "Present" ? "selected" : ""}>Present</option><option value="Absent" ${record?.status === "Absent" ? "selected" : ""}>Absent</option></select></div>`;
  }).join("") || "Add children first.";
}
async function history() {
  const params = new URLSearchParams(); if ($("history-child").value) params.set("childId", $("history-child").value);
  if ($("history-date").value) params.set("date", $("history-date").value);
  const items = await api(`/api/daily-care?${params}`);
  $("history-list").innerHTML = items.map(x => `<div class="item"><b>${esc(nameOf(x.childId))} · ${esc(x.date)}</b>
    <small>Meals: ${esc(x.meals)} · Nap: ${esc(x.nap)}</small><p>Activities: ${esc(x.activities)}</p><p>Incidents: ${esc(x.incidents)}</p><p>Notes: ${esc(x.notes)}</p>
    <button data-action="edit-daily" data-id="${esc(x.id)}">Edit</button><button data-action="delete-daily" data-id="${esc(x.id)}">Delete</button></div>`).join("") || "No daily records.";
  window.dailyRecords = items;
}
async function loadStaff() {
  staff = await api("/api/staff");
  $("staff-list").innerHTML = staff.map(x => `<div class="item"><b>${esc(x.name)}</b><small>${esc(x.role)} · ${esc(x.phone)} · ${esc(x.email)}</small>
  <button data-action="edit-staff" data-id="${esc(x.id)}">Edit</button><button data-action="delete-staff" data-id="${esc(x.id)}">Delete</button></div>`).join("") || "No staff yet.";
}
async function loadPayments() {
  payments = await api("/api/payments");
  $("payment-list").innerHTML = payments.map(x => `<div class="item"><b>${esc(nameOf(x.childId))}</b><small>${esc(x.date)} · $${Number(x.amount).toFixed(2)} · ${esc(x.method)} · ${esc(x.notes)}</small>
  <button data-action="delete-payment" data-id="${esc(x.id)}">Delete</button></div>`).join("") || "No payments yet.";
}
async function openTab(tab) {
  activeTab = tab;
  document.querySelectorAll(".panel").forEach(x => x.classList.toggle("active", x.id === tab));
  document.querySelectorAll("nav button").forEach(x => x.classList.toggle("active", x.dataset.tab === tab));
  try {
    if (tab === "dashboard") await dashboard();
    if (tab === "children") await loadChildren();
    if (tab === "attendance") await attendance();
    if (tab === "history") await history();
    if (tab === "staff") await loadStaff();
    if (tab === "payments") await loadPayments();
  } catch (err) { notify(err.message, true); }
}
document.querySelectorAll("nav button").forEach(button => button.addEventListener("click", () => openTab(button.dataset.tab)));
$("child-search").addEventListener("input", renderChildren);
$("child-form").addEventListener("submit", e => { e.preventDefault(); safe(async () => {
  const form = e.target, item = formData(form), id = item.id; delete item.id;
  await api(id ? `/api/children/${id}` : "/api/children", { method: id ? "PUT" : "POST", body: JSON.stringify(item) });
  resetForm(form); $("child-save").textContent = "Add Child"; await loadChildren();
}); });
$("child-cancel").addEventListener("click", () => { resetForm($("child-form")); $("child-save").textContent = "Add Child"; });
$("attendance-date").addEventListener("change", () => safe(async () => { await attendance(); }));
$("attendance-save").addEventListener("click", () => safe(async () => {
  const date = $("attendance-date").value;
  for (const select of document.querySelectorAll("[data-att-child]")) {
    if (select.value) await api("/api/attendance", { method:"PUT", body: JSON.stringify({ childId:select.dataset.attChild, date, status:select.value }) });
    else await api(`/api/attendance?childId=${encodeURIComponent(select.dataset.attChild)}&date=${encodeURIComponent(date)}`, {method:"DELETE"});
  }
  await dashboard(); await attendance();
}));
$("daily-form").addEventListener("submit", e => { e.preventDefault(); safe(async () => {
  const form = e.target, item = formData(form), id = item.id; delete item.id;
  await api(id ? `/api/daily-care/${id}` : "/api/daily-care", { method: id ? "PUT" : "POST", body: JSON.stringify(item) });
  resetForm(form); form.elements.namedItem("date").value = today();
}); });
$("daily-clear").addEventListener("click", () => { resetForm($("daily-form")); $("daily-form").elements.namedItem("date").value = today(); });
$("history-refresh").addEventListener("click", () => safe(history));
$("staff-form").addEventListener("submit", e => { e.preventDefault(); safe(async () => {
  const form = e.target, item = formData(form), id = item.id; delete item.id;
  await api(id ? `/api/staff/${id}` : "/api/staff", { method:id ? "PUT" : "POST", body:JSON.stringify(item) });
  resetForm(form); await loadStaff();
}); });
$("staff-clear").addEventListener("click", () => resetForm($("staff-form")));
$("payment-form").addEventListener("submit", e => { e.preventDefault(); safe(async () => {
  const item = formData(e.target); item.amount = Number(item.amount);
  await api("/api/payments", { method:"POST", body:JSON.stringify(item) });
  e.target.reset(); e.target.elements.namedItem("date").value = today(); await loadPayments();
}); });
document.addEventListener("click", e => { const button = e.target.closest("[data-action]"); if (!button) return;
  const {action, id} = button.dataset;
  safe(async () => {
    if (action === "edit-child") { fillForm($("child-form"), children.find(x => x.id === id)); $("child-save").textContent = "Update Child"; $("child-form").scrollIntoView(); return; }
    if (action === "edit-staff") { fillForm($("staff-form"), staff.find(x => x.id === id)); $("staff-form").scrollIntoView(); return; }
    if (action === "edit-daily") { fillForm($("daily-form"), window.dailyRecords.find(x => x.id === id)); await openTab("daily"); return; }
    if (!confirm("Delete this record?")) return;
    const resource = ({"delete-child":"children","delete-staff":"staff","delete-daily":"daily-care","delete-payment":"payments"})[action];
    if (!resource) return;
    await api(`/api/${resource}/${id}`, {method:"DELETE"});
    if (resource === "children") await loadChildren();
    if (resource === "staff") await loadStaff();
    if (resource === "daily-care") await history();
    if (resource === "payments") await loadPayments();
  });
});
(async () => {
  $("attendance-date").value = today();
  $("daily-form").elements.namedItem("date").value = today();
  $("payment-form").elements.namedItem("date").value = today();
  try { await loadChildren(); await dashboard(); } catch (err) { notify(`Cannot connect to API: ${err.message}`, true); }
})();
