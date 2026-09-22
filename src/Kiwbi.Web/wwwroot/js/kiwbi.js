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

  // Dropzones de subida de fichero (plano general/vivienda): muestra el nombre del fichero elegido.
  function initDropzoneFileNames() {
    document.querySelectorAll('.kiwbi-dropzone input[type="file"]').forEach(function (input) {
      var label = input.closest('.kiwbi-dropzone');
      var nameEl = label ? label.querySelector('.kiwbi-dropzone-filename') : null;
      if (!nameEl) return;

      var defaultText = nameEl.textContent;

      input.addEventListener('change', function () {
        nameEl.textContent = input.files && input.files.length ? input.files[0].name : defaultText;
      });
    });
  }

  // Mi promotora (EditBranding): sincroniza cada input[type=color] con su hex de texto adyacente,
  // y actualiza la variable CSS (--preview-accent/--preview-accent-2) que alimenta la vista previa.
  function initColorFieldSync() {
    document.querySelectorAll('[data-color-pair]').forEach(function (field) {
      var colorInput = field.querySelector('input[type="color"]');
      var hexInput = field.querySelector('input[type="text"]');
      var previewVar = field.dataset.previewVar;
      if (!colorInput || !hexInput) return;

      function applyPreview(value) {
        if (!previewVar) return;
        var form = field.closest('form');
        if (form) form.style.setProperty(previewVar, value);
      }

      hexInput.value = colorInput.value.toUpperCase();

      colorInput.addEventListener('input', function () {
        hexInput.value = colorInput.value.toUpperCase();
        applyPreview(colorInput.value);
      });

      hexInput.addEventListener('change', function () {
        if (/^#[0-9A-Fa-f]{6}$/.test(hexInput.value)) {
          colorInput.value = hexInput.value;
          applyPreview(hexInput.value);
        } else {
          hexInput.value = colorInput.value.toUpperCase();
        }
      });
    });
  }

  // Mi promotora (EditBranding): aplica una paleta sugerida a los dos selectores de color.
  function initPalettePresets() {
    var buttons = document.querySelectorAll('[data-palette]');
    if (!buttons.length) return;

    buttons.forEach(function (button) {
      button.addEventListener('click', function () {
        buttons.forEach(function (b) { b.classList.remove('active'); });
        button.classList.add('active');

        var primaryField = document.querySelector('[data-color-pair][data-preview-var="--preview-accent"]');
        var secondaryField = document.querySelector('[data-color-pair][data-preview-var="--preview-accent-2"]');

        [
          [primaryField, button.dataset.primary],
          [secondaryField, button.dataset.secondary],
        ].forEach(function (pair) {
          var field = pair[0];
          var value = pair[1];
          if (!field || !value) return;
          var colorInput = field.querySelector('input[type="color"]');
          if (!colorInput) return;
          colorInput.value = value;
          colorInput.dispatchEvent(new Event('input'));
        });
      });
    });
  }

  // Selects marcados con .kiwbi-status-select (p.ej. Estado en Viviendas): retinta el propio <select> con la
  // clase kiwbi-status-select-{suffix} de la opción elegida (cada <option> trae data-status-suffix).
  function initStatusSelectColors() {
    var selects = document.querySelectorAll('.kiwbi-status-select');
    if (!selects.length) return;

    function applySuffix(select) {
      var suffix = select.options[select.selectedIndex].dataset.statusSuffix;
      Array.prototype.slice.call(select.classList)
        .filter(function (c) { return c.indexOf('kiwbi-status-select-') === 0; })
        .forEach(function (c) { select.classList.remove(c); });
      if (suffix) select.classList.add('kiwbi-status-select-' + suffix);
    }

    selects.forEach(function (select) {
      applySuffix(select);
      select.addEventListener('change', function () { applySuffix(select); });
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initSidebarCollapse();
    initScopeSelector();
    initPillToggles();
    initDropzoneFileNames();
    initColorFieldSync();
    initPalettePresets();
    initStatusSelectColors();
  });
})();
