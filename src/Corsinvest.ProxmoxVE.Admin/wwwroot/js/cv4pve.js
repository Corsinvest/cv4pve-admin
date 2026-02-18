/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */

window.cv4pve = window.cv4pve || {};

/**
 * WidgetGrid Module - Generic widget grid component
 * Supports multiple instances identified by gridId
 */
cv4pve.WidgetGrid = (function () {
    const _instances = new Map();

    const MIN_CELL_WIDTH = 70;
    const MIN_CELL_HEIGHT = 50;

    function GridInstance(gridId, rows, cols, margin, editMode, showGrid, dotNetRef) {
        this.gridId = gridId;
        this.cfg = { rows, cols, margin };
        this.editMode = editMode;
        this.showGrid = showGrid;
        this.dotNetRef = dotNetRef;
        this.drag = null;
        this.resize = null;
        this.startCol = 0;
        this.startRow = 0;
        this.offsetX = 0;
        this.offsetY = 0;
        this.boundEvents = false;

        this._init();
    }

    GridInstance.prototype = {
        _init: function () {
            const g = document.getElementById(this.gridId);
            if (!g) return;

            const minGridWidth = MIN_CELL_WIDTH * this.cfg.cols + this.cfg.margin * 2;
            const minGridHeight = MIN_CELL_HEIGHT * this.cfg.rows + this.cfg.margin * 2;
            g.style.minWidth = minGridWidth + 'px';
            g.style.minHeight = minGridHeight + 'px';

            g.style.setProperty('--rows', this.cfg.rows);
            g.style.setProperty('--cols', this.cfg.cols);
            g.classList.toggle('editable', this.editMode);
            g.classList.toggle('show-grid', this.showGrid);
            this._updateAllWidgets();

            if (!this.boundEvents) {
                this._bindEvents();
            }
        },

        _getCellSize: function () {
            const g = document.getElementById(this.gridId);
            if (!g) return { w: 0, h: 0 };
            const rect = g.getBoundingClientRect();
            return {
                w: rect.width / this.cfg.cols,
                h: rect.height / this.cfg.rows
            };
        },

        _updateWidget: function (w) {
            const cell = this._getCellSize();
            const m = this.cfg.margin;
            w.style.left = (+w.dataset.col * cell.w + m) + 'px';
            w.style.top = (+w.dataset.row * cell.h + m) + 'px';
            w.style.width = (+w.dataset.cs * cell.w - m * 2) + 'px';
            w.style.height = (+w.dataset.rs * cell.h - m * 2) + 'px';
        },

        _updateAllWidgets: function () {
            const g = document.getElementById(this.gridId);
            if (!g) return;
            g.querySelectorAll('.cv4pve-widgetgrid-widget').forEach(w => this._updateWidget(w));
        },

        _bindEvents: function () {
            const self = this;
            const g = document.getElementById(this.gridId);
            if (!g) return;

            g.addEventListener('mousedown', function (e) {
                if (!self.editMode) return;

                if (e.target.classList.contains('cv4pve-widgetgrid-resize')) {
                    const w = e.target.closest('.cv4pve-widgetgrid-widget');
                    if (w) {
                        self.resize = w;
                        self.startCol = +w.dataset.col;
                        self.startRow = +w.dataset.row;
                        e.preventDefault();
                    }
                    return;
                }

                const header = e.target.closest('.cv4pve-widgetgrid-header');
                if (header && !e.target.closest('button') && !e.target.closest('.rz-button')) {
                    const w = header.closest('.cv4pve-widgetgrid-widget');
                    if (w && w.classList.contains('editable')) {
                        self.drag = w;
                        const wRect = w.getBoundingClientRect();
                        self.offsetX = e.clientX - wRect.left;
                        self.offsetY = e.clientY - wRect.top;
                        w.classList.add('dragging');
                        e.preventDefault();
                    }
                }
            });

            document.addEventListener('mousemove', function (e) {
                if (self.drag) {
                    const g = document.getElementById(self.gridId);
                    if (!g) return;
                    const gRect = g.getBoundingClientRect();
                    const cell = self._getCellSize();
                    const x = e.clientX - gRect.left - self.offsetX;
                    const y = e.clientY - gRect.top - self.offsetY;
                    const c = Math.max(0, Math.min(self.cfg.cols - +self.drag.dataset.cs, Math.round(x / cell.w)));
                    const r = Math.max(0, Math.min(self.cfg.rows - +self.drag.dataset.rs, Math.round(y / cell.h)));
                    self.drag.dataset.col = c;
                    self.drag.dataset.row = r;
                    self._updateWidget(self.drag);
                }

                if (self.resize) {
                    const g = document.getElementById(self.gridId);
                    if (!g) return;
                    const gRect = g.getBoundingClientRect();
                    const cell = self._getCellSize();
                    const c = Math.floor((e.clientX - gRect.left) / cell.w);
                    const r = Math.floor((e.clientY - gRect.top) / cell.h);
                    const cs = Math.max(1, Math.min(self.cfg.cols - self.startCol, c - self.startCol + 1));
                    const rs = Math.max(1, Math.min(self.cfg.rows - self.startRow, r - self.startRow + 1));
                    self.resize.dataset.cs = cs;
                    self.resize.dataset.rs = rs;
                    self._updateWidget(self.resize);
                }
            });

            document.addEventListener('mouseup', function () {
                if (!self.drag && !self.resize) return;

                const w = self.drag || self.resize;
                if (self.dotNetRef && w) {
                    self.dotNetRef.invokeMethodAsync('UpdateWidgetPosition',
                        +w.dataset.id, +w.dataset.col, +w.dataset.row, +w.dataset.cs, +w.dataset.rs);
                }

                if (self.drag) {
                    self.drag.classList.remove('dragging');
                    self.drag = null;
                }
                self.resize = null;
            });

            window.addEventListener('resize', function () {
                self._updateAllWidgets();
            });

            this.boundEvents = true;
        },

        setEditMode: function (editMode) {
            this.editMode = editMode;
            const g = document.getElementById(this.gridId);
            if (g) {
                g.classList.toggle('editable', editMode);
            }
        },

        setShowGrid: function (showGrid) {
            this.showGrid = showGrid;
            const g = document.getElementById(this.gridId);
            if (g) {
                g.classList.toggle('show-grid', showGrid);
            }
        },

        updateConfig: function (rows, cols, margin, editMode, showGrid) {
            console.log('[WidgetGrid] updateConfig', this.gridId, { rows, cols, margin, editMode, showGrid });
            this.cfg = { rows, cols, margin };
            this.editMode = editMode;
            this.showGrid = showGrid;
            this._init();
        },

        destroy: function () {
            this.dotNetRef = null;
        }
    };

    return {
        createInstance: function (gridId, rows, cols, margin, editMode, showGrid, dotNetRef) {
            console.log('[WidgetGrid] createInstance', gridId, { rows, cols, margin, editMode, showGrid });
            if (_instances.has(gridId)) {
                _instances.get(gridId).destroy();
            }
            const instance = new GridInstance(gridId, rows, cols, margin, editMode, showGrid, dotNetRef);
            _instances.set(gridId, instance);
        },

        destroyInstance: function (gridId) {
            if (_instances.has(gridId)) {
                _instances.get(gridId).destroy();
                _instances.delete(gridId);
            }
        },

        setEditMode: function (gridId, editMode) {
            const instance = _instances.get(gridId);
            if (instance) {
                instance.setEditMode(editMode);
            }
        },

        updateConfig: function (gridId, rows, cols, margin, editMode, showGrid) {
            const instance = _instances.get(gridId);
            if (instance) {
                instance.updateConfig(rows, cols, margin, editMode, showGrid);
            }
        },

        refreshWidgets: function (gridId) {
            console.log('[WidgetGrid] refreshWidgets', gridId);
            const instance = _instances.get(gridId);
            if (instance) {
                instance._updateAllWidgets();
            }
        }
    };
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
