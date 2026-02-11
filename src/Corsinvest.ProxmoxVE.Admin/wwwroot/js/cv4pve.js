/**
 * CV4PVE Admin - JavaScript Library
 * Corsinvest Srl - https://corsinvest.it
 *
 * Main namespace for all application JavaScript components.
 * Modular structure supporting multiple instances per component.
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
