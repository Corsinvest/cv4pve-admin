/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

window.cv4pve = window.cv4pve || {};

cv4pve.SideDialog = (function () {
    const _instances = new Map();

    function positionBar(bar, dialog) {
        const r = dialog.getBoundingClientRect();
        bar.style.top = r.top + 'px';
        bar.style.height = r.height + 'px';
        bar.style.left = r.left + 'px';
    }

    function attach(dialog) {
        if (dialog.dataset.resizerAttached) return;
        dialog.dataset.resizerAttached = 'true';

        setTimeout(function () {
            const bar = document.createElement('div');
            bar.className = 'rz-dialog-resize-bar cv4pve-side-dialog-resizer';
            bar.title = 'Drag to resize';
            bar.setAttribute('aria-label', 'Resize side dialog');
            const span = document.createElement('span');
            span.className = 'rz-resize';
            span.setAttribute('aria-hidden', 'true');
            bar.appendChild(span);
            document.body.appendChild(bar);

            positionBar(bar, dialog);

            const resizer = Radzen.createSideDialogResizer(bar, dialog, { position: 'right', minWidth: 300 });

            // Keep bar aligned when dialog resizes or window resizes
            const ro = new ResizeObserver(() => positionBar(bar, dialog));
            ro.observe(dialog);
            window.addEventListener('resize', () => positionBar(bar, dialog));

            _instances.set(dialog, { resizer, bar, ro });
        }, 200);
    }

    function detach(dialog) {
        const inst = _instances.get(dialog);
        if (inst) {
            inst.resizer?.dispose();
            inst.ro?.disconnect();
            inst.bar?.remove();
            _instances.delete(dialog);
        }
    }

    const observer = new MutationObserver(mutations => {
        mutations.forEach(m => {
            m.addedNodes.forEach(node => {
                if (node.nodeType === 1) {
                    if (node.classList.contains('cv4pve-side-dialog-resizable')) attach(node);
                    node.querySelectorAll?.('.cv4pve-side-dialog-resizable').forEach(attach);
                }
            });
            m.removedNodes.forEach(node => {
                if (node.nodeType === 1) {
                    if (node.classList.contains('cv4pve-side-dialog-resizable')) detach(node);
                    node.querySelectorAll?.('.cv4pve-side-dialog-resizable').forEach(detach);
                }
            });
        });
    });

    observer.observe(document.body, { childList: true, subtree: true });
})();

/**
 * Clipboard helper with fallback for non-secure contexts (HTTP on LAN IP).
 * `navigator.clipboard` is only exposed on HTTPS / localhost / 127.0.0.1.
 * For everything else (http://192.168.x.x:8080, common LAN deployment)
 * we fall back to the legacy `document.execCommand('copy')` via a hidden textarea.
 */
cv4pve.clipboard = {
    writeText: function (text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.setAttribute('readonly', '');
        ta.style.position = 'fixed';
        ta.style.top = '0';
        ta.style.left = '0';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.select();
        try {
            document.execCommand('copy');
        } finally {
            document.body.removeChild(ta);
        }
    }
};
