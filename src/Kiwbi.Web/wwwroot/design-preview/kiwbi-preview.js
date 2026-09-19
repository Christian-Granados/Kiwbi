// Feature 8.1 — comportamiento compartido por los PoC de wwwroot/design-preview/.
// Puro vanilla JS, sin dependencias: en la implementación real (Feature 8.2/8.3) esto se
// sustituiría por Alpine.js/HTMX donde corresponda, siguiendo el stack ya decidido.
(function () {
  'use strict';

  var SIDEBAR_STORAGE_KEY = 'kiwbi-poc-sidebar-collapsed';

  function initSidebarCollapse() {
    var sidebar = document.getElementById('kiwbiSidebar');
    var collapseBtn = document.getElementById('kiwbiCollapseBtn');
    if (!sidebar || !collapseBtn) return;

    function applyCollapsed(collapsed) {
      sidebar.classList.toggle('collapsed', collapsed);
    }

    applyCollapsed(localStorage.getItem(SIDEBAR_STORAGE_KEY) === '1');

    collapseBtn.addEventListener('click', function () {
      var collapsed = !sidebar.classList.contains('collapsed');
      applyCollapsed(collapsed);
      localStorage.setItem(SIDEBAR_STORAGE_KEY, collapsed ? '1' : '0');
    });
  }

  function initTenantSwitcher() {
    var swatches = document.querySelectorAll('.kiwbi-swatch');
    if (!swatches.length) return;

    swatches.forEach(function (btn) {
      btn.addEventListener('click', function () {
        document.documentElement.style.setProperty('--kiwbi-accent', btn.dataset.accent);
        document.documentElement.style.setProperty('--kiwbi-accent-2', btn.dataset.accent2);

        var mark = document.getElementById('kiwbiMark');
        var name = document.getElementById('kiwbiTenantName');
        if (mark) mark.textContent = btn.dataset.initial;
        if (name) name.textContent = btn.dataset.name;
      });
    });
  }

  // Grupo de botones "Se aplica a" (Toda la promoción / Tipología / Vivienda): marca el activo
  // y muestra/oculta el selector múltiple condicional correspondiente (data-scope-target).
  function initScopeSelector() {
    var options = document.querySelectorAll('.kiwbi-scope-option');
    if (!options.length) return;

    options.forEach(function (option) {
      option.addEventListener('click', function () {
        options.forEach(function (o) { o.classList.remove('active'); });
        option.classList.add('active');

        document.querySelectorAll('[data-scope-panel]').forEach(function (panel) {
          panel.hidden = panel.dataset.scopePanel !== option.dataset.scope;
        });
      });
    });
  }

  // Pills de selección múltiple (tipologías/viviendas): toggle visual, sin estado persistido.
  function initPillToggles() {
    document.querySelectorAll('.kiwbi-pill').forEach(function (pill) {
      pill.addEventListener('click', function () {
        pill.classList.toggle('active');
      });
    });
  }

  // Tarjetas de opción seleccionables (detalle de Vivienda, Comprador): una activa por grupo.
  function initOptionCards() {
    document.querySelectorAll('[data-option-group]').forEach(function (card) {
      card.addEventListener('click', function () {
        if (card.disabled) return;

        var group = card.dataset.optionGroup;
        document.querySelectorAll('[data-option-group="' + group + '"]').forEach(function (sibling) {
          sibling.classList.remove('selected');
        });
        card.classList.add('selected');
      });
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initSidebarCollapse();
    initTenantSwitcher();
    initScopeSelector();
    initPillToggles();
    initOptionCards();
  });
})();
