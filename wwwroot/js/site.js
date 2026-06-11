/* ── Toast Notifications ─────────────────────── */
function showToast(message, type, duration) {
    type = type || 'success';
    duration = duration || 3000;
    var container = document.getElementById('toast-container');
    if (!container) return;

    var icons = {
        success: 'bi-check-circle-fill',
        error: 'bi-x-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        info: 'bi-info-circle-fill'
    };

    var toast = document.createElement('div');
    toast.className = 'is-toast toast-' + type;
    toast.innerHTML = '<i class="bi ' + (icons[type] || icons.info) + '"></i><span>' + message + '</span>';
    container.appendChild(toast);

    setTimeout(function () {
        toast.classList.add('toast-hiding');
        setTimeout(function () { toast.remove(); }, 280);
    }, duration);
}

/* ── Theme Toggle ────────────────────────────── */
(function initTheme() {
    var saved = localStorage.getItem('invenscan-theme') || 'light';
    document.documentElement.setAttribute('data-theme', saved);
    updateThemeIcon(saved);
})();

function updateThemeIcon(theme) {
    var btn = document.getElementById('themeToggleBtn');
    if (!btn) return;
    btn.innerHTML = theme === 'dark'
        ? '<i class="bi bi-sun-fill"></i>'
        : '<i class="bi bi-moon-fill"></i>';
    btn.title = theme === 'dark' ? 'Switch to Light Mode' : 'Switch to Dark Mode';
}

function toggleTheme() {
    var current = document.documentElement.getAttribute('data-theme') || 'light';
    var next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('invenscan-theme', next);
    updateThemeIcon(next);
}

/* ── Sidebar Toggle ──────────────────────────── */
(function initSidebar() {
    var saved = localStorage.getItem('invenscan-sidebar') || 'expanded';
    var sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    if (saved === 'collapsed') sidebar.classList.add('collapsed');
    updateSidebarIcon(saved);
})();

function updateSidebarIcon(state) {
    var btn = document.getElementById('sidebarToggleBtn');
    if (!btn) return;
    btn.innerHTML = state === 'collapsed'
        ? '<i class="bi bi-layout-sidebar-reverse"></i>'
        : '<i class="bi bi-layout-sidebar"></i>';
}

function toggleSidebar() {
    var sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    var isCollapsed = sidebar.classList.toggle('collapsed');
    var state = isCollapsed ? 'collapsed' : 'expanded';
    localStorage.setItem('invenscan-sidebar', state);
    updateSidebarIcon(state);
}

/* ── AJAX Form Submit for Modals ─────────────── */
function bindModalForm(formId, modalId, onSuccess) {
    var form = document.getElementById(formId);
    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        var submitBtn = form.querySelector('[type="submit"]');
        var originalText = submitBtn ? submitBtn.innerHTML : '';
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';
        }

        var token = (form.querySelector('[name="__RequestVerificationToken"]') || {}).value || '';
        var formData = new FormData(form);

        fetch(form.action, {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': token },
            body: formData
        })
        .then(function (r) { return r.json(); })
        .then(function (result) {
            if (result.success) {
                var modalEl = document.getElementById(modalId);
                var modal = modalEl ? bootstrap.Modal.getInstance(modalEl) : null;
                if (modal) modal.hide();
                showToast(result.message, 'success');
                if (onSuccess) { onSuccess(); }
                else { setTimeout(function () { location.reload(); }, 600); }
            } else {
                showToast(result.message || 'An error occurred.', 'error');
            }
        })
        .catch(function () {
            showToast('Request failed. Please try again.', 'error');
        })
        .finally(function () {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        });
    });
}

/* ── Delete Confirmation Helper ─────────────── */
function setupDeleteButtons(btnSelector, modalId, formId, labelId) {
    document.querySelectorAll(btnSelector).forEach(function (btn) {
        btn.addEventListener('click', function () {
            var label = this.dataset.label || 'this item';
            var deleteUrl = this.dataset.deleteUrl;
            var form = document.getElementById(formId);
            var labelEl = document.getElementById(labelId);
            if (labelEl) labelEl.textContent = label;
            if (form) form.action = deleteUrl;
            new bootstrap.Modal(document.getElementById(modalId)).show();
        });
    });
}

/* ── Auto-dismiss alerts ─────────────────────── */
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-is').forEach(function (el) {
        setTimeout(function () {
            el.style.opacity = '0';
            el.style.transition = 'opacity 0.4s';
            setTimeout(function () { el.remove(); }, 400);
        }, 4000);
    });
});
