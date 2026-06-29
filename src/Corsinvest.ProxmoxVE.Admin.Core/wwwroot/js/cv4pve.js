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
 * RangeSelector Module - Dual-handle range slider with data-driven labels
 * Supports multiple instances identified by id
 */
cv4pve.RangeSelector = (function () {
    const _instances = new Map();

    // labels: array of strings, one per index position (evenly distributed 0..100%)
    // realtimeUpdate: if true, invokes .NET during drag; if false (default), only on release
    // debounceMs: debounce delay in ms for realtimeUpdate (0 = no debounce)
    function RangeSelectorInstance(id, dotNetRef, startPct, endPct, labels, realtimeUpdate, debounceMs) {
        this.dotNetRef = dotNetRef;
        this.track = document.getElementById(id);
        // body is the parent of the track-wrapper; handles and labels live there
        this.body = this.track.closest('.cv4pve-range-selector-body');
        this.rs = this.track.closest('.cv4pve-range-selector');
        this.handleStart = this.body.querySelector('.cv4pve-range-selector-handle-start');
        this.handleEnd = this.body.querySelector('.cv4pve-range-selector-handle-end');
        this.selection = this.track.querySelector('.cv4pve-range-selector-selection');
        this.labelStart = this.rs.querySelector('.cv4pve-range-selector-label-start');
        this.labelEnd = this.rs.querySelector('.cv4pve-range-selector-label-end');
        this.labels = labels || [];
        this.realtimeUpdate = !!realtimeUpdate;
        this.debounceMs = debounceMs || 0;
        this._debounceTimer = null;

        this._startPct = startPct;
        this._endPct = endPct;
        this._lastNotifiedStart = startPct;
        this._lastNotifiedEnd = endPct;
        this._dragging = null;      // 'start' | 'end' | 'range'
        this._rangeDragStartX = 0;
        this._rangeDragStartPct = 0;
        this._rangeDragEndPct = 0;
        this._onMouseMove = (e) => this._move(e);
        this._onMouseUp = () => this._stopDrag();
        this._onTouchMove = (e) => { e.preventDefault(); this._move(e); };
        this._onTouchEnd = () => this._stopDrag();

        this.handleStart.addEventListener('mousedown', (e) => { e.stopPropagation(); this._startDrag(e, 'start'); });
        this.handleEnd.addEventListener('mousedown', (e) => { e.stopPropagation(); this._startDrag(e, 'end'); });
        this.handleStart.addEventListener('touchstart', (e) => { e.stopPropagation(); this._startDrag(e, 'start'); }, { passive: true });
        this.handleEnd.addEventListener('touchstart', (e) => { e.stopPropagation(); this._startDrag(e, 'end'); }, { passive: true });
        // Click anywhere in the body between the two handles → drag the whole range
        this.body.addEventListener('mousedown', (e) => {
            const rect = this.body.getBoundingClientRect();
            const pct = (this._getClientX(e) - rect.left) / rect.width * 100;
            if (pct > this._startPct && pct < this._endPct) this._startDrag(e, 'range');
        });
        this.body.addEventListener('touchstart', (e) => {
            const rect = this.body.getBoundingClientRect();
            const pct = (this._getClientX(e) - rect.left) / rect.width * 100;
            if (pct > this._startPct && pct < this._endPct) this._startDrag(e, 'range');
        }, { passive: true });

        this._render();
    }

    RangeSelectorInstance.prototype._notifyDotNet = function () {
        this._lastNotifiedStart = this._startPct;
        this._lastNotifiedEnd = this._endPct;
        this.dotNetRef.invokeMethodAsync('OnRangeChangedJs', this._startPct, this._endPct);
    };

    RangeSelectorInstance.prototype._labelAt = function (pct) {
        if (!this.labels.length) return '';
        const idx = Math.round(pct / 100 * (this.labels.length - 1));
        return this.labels[Math.max(0, Math.min(idx, this.labels.length - 1))];
    };

    RangeSelectorInstance.prototype._startDrag = function (e, which) {
        this._dragging = which;
        if (which === 'range') {
            this._rangeDragStartX = this._getClientX(e);
            this._rangeDragStartPct = this._startPct;
            this._rangeDragEndPct = this._endPct;
        }
        document.addEventListener('mousemove', this._onMouseMove);
        document.addEventListener('mouseup', this._onMouseUp);
        document.addEventListener('touchmove', this._onTouchMove, { passive: false });
        document.addEventListener('touchend', this._onTouchEnd);
        e.preventDefault && e.preventDefault();
    };

    RangeSelectorInstance.prototype._getClientX = function (e) {
        return e.touches ? e.touches[0].clientX : e.clientX;
    };

    RangeSelectorInstance.prototype._move = function (e) {
        if (!this._dragging) return;
        const rect = this.body.getBoundingClientRect();

        if (this._dragging === 'range') {
            const dx = this._getClientX(e) - this._rangeDragStartX;
            const deltaPct = dx / rect.width * 100;
            const span = this._rangeDragEndPct - this._rangeDragStartPct;
            let newStart = this._rangeDragStartPct + deltaPct;
            newStart = Math.max(0, Math.min(100 - span, newStart));
            this._startPct = newStart;
            this._endPct = newStart + span;
        } else {
            let pct = (this._getClientX(e) - rect.left) / rect.width * 100;
            pct = Math.max(0, Math.min(100, pct));
            if (this._dragging === 'start') {
                this._startPct = Math.min(pct, this._endPct - 1);
            } else {
                this._endPct = Math.max(pct, this._startPct + 1);
            }
        }
        // Only notify if values actually changed
        const changed = this._startPct !== this._lastNotifiedStart || this._endPct !== this._lastNotifiedEnd;
        this._render();
        if (this.realtimeUpdate && changed) {
            if (this.debounceMs > 0) {
                clearTimeout(this._debounceTimer);
                this._debounceTimer = setTimeout(() => {
                    this._notifyDotNet();
                }, this.debounceMs);
            } else {
                this._notifyDotNet();
            }
        }
    };

    RangeSelectorInstance.prototype._stopDrag = function () {
        if (this._dragging) {
            clearTimeout(this._debounceTimer);
            const changed = this._startPct !== this._lastNotifiedStart || this._endPct !== this._lastNotifiedEnd;
            if (changed || !this.realtimeUpdate) {
                this._notifyDotNet();
            }
        }
        this._dragging = null;
        document.removeEventListener('mousemove', this._onMouseMove);
        document.removeEventListener('mouseup', this._onMouseUp);
        document.removeEventListener('touchmove', this._onTouchMove);
        document.removeEventListener('touchend', this._onTouchEnd);
    };

    RangeSelectorInstance.prototype._render = function () {
        // Handles are in the body — position relative to body width
        this.handleStart.style.left = this._startPct + '%';
        this.handleEnd.style.left = this._endPct + '%';
        // Selection bar is inside the track
        this.selection.style.left = this._startPct + '%';
        this.selection.style.width = (this._endPct - this._startPct) + '%';
        // Labels: clamp to edges and push apart if overlapping
        if (this.labelStart && this.labelEnd) {
            this.labelStart.textContent = this._labelAt(this._startPct);
            this.labelEnd.textContent = this._labelAt(this._endPct);

            const containerW = this.rs.getBoundingClientRect().width;
            if (containerW > 0) {
                const wS = this.labelStart.offsetWidth;
                const wE = this.labelEnd.offsetWidth;
                const halfS = wS / 2 / containerW * 100;
                const halfE = wE / 2 / containerW * 100;

                // Clamp each label to stay inside the container
                let posS = Math.max(halfS, Math.min(100 - halfS, this._startPct));
                let posE = Math.max(halfE, Math.min(100 - halfE, this._endPct));

                // Push apart if overlapping
                const gap = (wS / 2 + wE / 2 + 4) / containerW * 100;
                if (posE - posS < gap) {
                    const mid = (posS + posE) / 2;
                    posS = Math.max(halfS, mid - gap / 2);
                    posE = Math.min(100 - halfE, mid + gap / 2);
                }

                this.labelStart.style.left = posS + '%';
                this.labelEnd.style.left = posE + '%';
            } else {
                this.labelStart.style.left = this._startPct + '%';
                this.labelEnd.style.left = this._endPct + '%';
            }
        }
        // Drive mask widths via CSS custom properties on the body element
        this.body.style.setProperty('--rs-start', this._startPct + '%');
        this.body.style.setProperty('--rs-end', (100 - this._endPct) + '%');
    };

    RangeSelectorInstance.prototype.destroy = function () {
        this._dragging = null;
        clearTimeout(this._debounceTimer);
        document.removeEventListener('mousemove', this._onMouseMove);
        document.removeEventListener('mouseup', this._onMouseUp);
        document.removeEventListener('touchmove', this._onTouchMove);
        document.removeEventListener('touchend', this._onTouchEnd);
    };

    return {
        createInstance: function (id, dotNetRef, startPct, endPct, labels, realtimeUpdate, debounceMs) {
            if (_instances.has(id)) { _instances.get(id).destroy(); }
            _instances.set(id, new RangeSelectorInstance(id, dotNetRef, startPct, endPct, labels, realtimeUpdate, debounceMs));
        },

        destroyInstance: function (id) {
            if (_instances.has(id)) {
                _instances.get(id).destroy();
                _instances.delete(id);
            }
        }
    };
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
