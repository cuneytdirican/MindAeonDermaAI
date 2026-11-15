/* ============================================================
   Smart-Select — searchable single-select with INLINE "add new".
   Users never free-type an entity value; they select, and can
   define a new lookup value without leaving the screen.
   Markup contract (rendered by _SmartSelect.cshtml):
     .smartselect[data-entity][data-allow-create]
       input[type=hidden]         -> bound FK value
       .ss-control > .ss-value
       .ss-panel > .ss-search, .ss-list, .ss-add(.ss-create)
   ============================================================ */
(function () {
  function token() {
    var el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
  }

  function debounce(fn, ms) {
    var t; return function () { clearTimeout(t); var a = arguments, c = this; t = setTimeout(function () { fn.apply(c, a); }, ms); };
  }

  function init(root) {
    if (root.__ssReady) return; root.__ssReady = true;
    var entity = root.dataset.entity;
    var allowCreate = root.dataset.allowCreate !== 'false';
    var hidden = root.querySelector('input[type=hidden]');
    var control = root.querySelector('.ss-control');
    var valueEl = root.querySelector('.ss-value');
    var panel = root.querySelector('.ss-panel');
    var search = root.querySelector('.ss-search');
    var list = root.querySelector('.ss-list');
    var addWrap = root.querySelector('.ss-add');
    var createBox = root.querySelector('.ss-create');
    var placeholder = valueEl ? valueEl.textContent : 'Select…';

    function open() {
      root.classList.add('open');
      search.value = '';
      if (createBox) createBox.classList.remove('show');
      load('');
      setTimeout(function () { search.focus(); }, 10);
    }
    function close() { root.classList.remove('open'); }

    function setValue(id, name) {
      hidden.value = id;
      valueEl.textContent = name;
      valueEl.classList.remove('placeholder');
      root.dispatchEvent(new CustomEvent('ss:change', { detail: { id: id, name: name }, bubbles: true }));
      close();
    }

    function render(items, q) {
      list.innerHTML = '';
      if (!items.length) {
        list.innerHTML = '<div class="ss-opt" style="cursor:default;color:var(--muted)">No matches.</div>';
      }
      items.forEach(function (it) {
        var d = document.createElement('div');
        d.className = 'ss-opt';
        d.innerHTML = '<span>' + escapeHtml(it.name) + (it.description ? ' <small>' + escapeHtml(it.description) + '</small>' : '') +
          '</span>' + (it.isSystem ? '<span class="ss-sys">system</span>' : '');
        d.addEventListener('click', function () { setValue(it.id, it.name); });
        list.appendChild(d);
      });
      // Prime the inline "Add new" with the current query.
      if (allowCreate && addWrap) {
        var nameInput = createBox.querySelector('.ss-newname');
        if (nameInput && q) nameInput.value = q;
      }
    }

    var load = debounce(function (q) {
      fetch('/api/lookup/' + encodeURIComponent(entity) + '?q=' + encodeURIComponent(q || ''), { headers: { 'Accept': 'application/json' } })
        .then(function (r) { return r.ok ? r.json() : []; })
        .then(function (items) { render(items, q); })
        .catch(function () { render([], q); });
    }, 160);

    control.addEventListener('click', function () { root.classList.contains('open') ? close() : open(); });
    control.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); open(); } });
    if (search) search.addEventListener('input', function () { load(search.value.trim()); });

    document.addEventListener('click', function (e) { if (!root.contains(e.target)) close(); });

    if (allowCreate && addWrap) {
      var addBtn = addWrap.querySelector('.ss-addbtn');
      var saveBtn = createBox.querySelector('.ss-savebtn');
      var errEl = createBox.querySelector('.ss-err');
      addBtn.addEventListener('click', function () {
        createBox.classList.toggle('show');
        var n = createBox.querySelector('.ss-newname');
        if (!n.value) n.value = search.value.trim();
        n.focus();
      });
      saveBtn.addEventListener('click', function () {
        var name = createBox.querySelector('.ss-newname').value.trim();
        var desc = createBox.querySelector('.ss-newdesc').value.trim();
        errEl.textContent = '';
        if (!name) { errEl.textContent = 'Please enter a name.'; return; }
        saveBtn.disabled = true;
        fetch('/api/lookup/' + encodeURIComponent(entity), {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
          body: JSON.stringify({ name: name, description: desc })
        }).then(function (r) { return r.json(); })
          .then(function (res) {
            saveBtn.disabled = false;
            if (res.success) { setValue(res.item.id, res.item.name); }
            else { errEl.textContent = res.error || 'Could not create.'; }
          }).catch(function () { saveBtn.disabled = false; errEl.textContent = 'Network error.'; });
      });
    }
  }

  function escapeHtml(s) { return (s || '').replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }

  function boot() { document.querySelectorAll('.smartselect').forEach(init); }
  if (document.readyState !== 'loading') boot(); else document.addEventListener('DOMContentLoaded', boot);
  window.MindAeonSmartSelect = { boot: boot, init: init };
})();
