// Feature 8.2 — comportamiento compartido del sistema de diseño Kiwbi (sidebar/formularios).
// Puro vanilla JS, sin dependencias; en puntos concretos ya existentes (selección de opciones del
// comprador) se usa HTMX en su lugar, ver Views/Buyer/HousingUnit.cshtml.
(function () {
  'use strict';

  var SIDEBAR_STORAGE_KEY = 'kiwbi-sidebar-collapsed';

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

  // Grupo de botones "Se aplica a" (Toda la promoción / Tipología / Vivienda): marca el activo
  // y muestra/oculta el selector múltiple condicional correspondiente (data-scope-panel).
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

  document.addEventListener('DOMContentLoaded', function () {
    initSidebarCollapse();
    initScopeSelector();
    initPillToggles();
  });
})();
