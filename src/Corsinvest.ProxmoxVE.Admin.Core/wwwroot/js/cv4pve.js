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

    // The dialog is anchored to the right edge: dragging the bar to the left widens it
    function createResizer(bar, dialog, minWidth) {
        let start = null;

        const onMove = e => {
            if (!start) return;
            dialog.style.width = Math.round(Math.max(minWidth, start.width - (e.clientX - start.x))) + 'px';
        };

        const onUp = () => {
            start = null;
            document.removeEventListener('pointermove', onMove);
        };

        const onDown = e => {
            e.preventDefault();
            start = { x: e.clientX, width: dialog.clientWidth };
            document.addEventListener('pointermove', onMove);
            document.addEventListener('pointerup', onUp, { once: true });
            document.addEventListener('pointercancel', onUp, { once: true });
        };

        bar.addEventListener('pointerdown', onDown);

        return {
            dispose() {
                bar.removeEventListener('pointerdown', onDown);
                document.removeEventListener('pointermove', onMove);
            }
        };
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

            const resizer = createResizer(bar, dialog, 300);

            // Keep bar aligned when dialog resizes or window resizes
            const ro = new ResizeObserver(() => positionBar(bar, dialog));
            ro.observe(dialog);
            const onResize = () => positionBar(bar, dialog);
            window.addEventListener('resize', onResize);

            _instances.set(dialog, { resizer, bar, ro, onResize });
        }, 200);
    }

    function detach(dialog) {
        const inst = _instances.get(dialog);
        if (inst) {
            inst.resizer?.dispose();
            inst.ro?.disconnect();
            window.removeEventListener('resize', inst.onResize);
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
 * Brings the selected result of the command palette into view: the arrows move the selection
 * from code while the focus stays in the search box, so the list does not scroll by itself.
 */
cv4pve.commandPalette = {
    scrollToSelected: function (containerId) {
        const container = document.getElementById(containerId);
        const item = container && container.querySelector('.rz-state-highlight, [aria-selected="true"]');
        if (item) { item.scrollIntoView({ block: 'nearest' }); }
    }
};

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
