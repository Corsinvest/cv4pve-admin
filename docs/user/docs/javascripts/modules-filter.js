// Modules index — category chips + edition chips + free-text filter.
// Categories are inferred from the link href; edition from .ee badge.
(function () {
  // Keep in sync with the CSS :has() rules in extra.css.
  var CATEGORIES = {
    protection: ["autosnap", "node-protect", "ups-monitor", "ddr"],
    health: ["backup-analytics", "replication-analytics", "diagnostics",
             "resources", "metrics-exporter", "update-manager", "vm-performance"],
    control: ["bots"],
    automation: ["workflow"],
    utilities: ["ai-server", "ai-assistant", "system-report"],
    management: ["portal"],
    features: ["dashboard", "command-palette"]
  };

  function slugOf(card) {
    var a = card.querySelector("a[href]");
    if (a) {
      var href = a.getAttribute("href") || "";
      var m = href.match(/([^/]+?)\/?$/);
      if (m) { return m[1]; }
    }
    var strong = card.querySelector("strong, p strong");
    return strong ? strong.textContent.trim().toLowerCase().replace(/\s+/g, "-") : "";
  }

  function init() {
    var filterRoot = document.querySelector(".modules-filter");
    var grid = document.querySelector(".modules-grid");
    if (!filterRoot || !grid) { return; }

    var cards = grid.querySelectorAll("li");
    var catChips = filterRoot.querySelectorAll("[data-cat]");
    var editionChips = filterRoot.querySelectorAll("[data-edition]");
    var search = filterRoot.querySelector('input[type="search"]');

    function apply() {
      var activeCatChip = filterRoot.querySelector("[data-cat].active");
      var cat = activeCatChip ? activeCatChip.getAttribute("data-cat") : "all";
      var activeEditionChip = filterRoot.querySelector("[data-edition].active");
      var edition = activeEditionChip ? activeEditionChip.getAttribute("data-edition") : "all";
      var q = (search.value || "").trim().toLowerCase();
      var allowed = cat === "all"
        ? null
        : (Object.prototype.hasOwnProperty.call(CATEGORIES, cat) ? CATEGORIES[cat] : []);

      cards.forEach(function (card) {
        var slug = slugOf(card);
        var hasEe = !!card.querySelector(".ee");
        var catOk = allowed === null || allowed.indexOf(slug) > -1;
        var textOk = !q || (card.textContent || "").toLowerCase().indexOf(q) > -1;
        var editionOk = edition === "all" ||
                        (edition === "ee" && hasEe) ||
                        (edition === "ce" && !hasEe);
        card.style.display = (catOk && textOk && editionOk) ? "" : "none";
      });
    }

    function bindChips(chips, attr) {
      chips.forEach(function (chip) {
        if (chip.__bound) { return; }
        chip.__bound = true;
        chip.addEventListener("click", function () {
          chips.forEach(function (c) { c.classList.remove("active"); });
          chip.classList.add("active");
          apply();
        });
      });
    }

    bindChips(catChips, "data-cat");
    bindChips(editionChips, "data-edition");

    if (search && !search.__bound) {
      search.__bound = true;
      search.addEventListener("input", apply);
    }

    // Defaults
    var defaultCat = filterRoot.querySelector('[data-cat="all"]');
    if (defaultCat && !filterRoot.querySelector("[data-cat].active")) {
      defaultCat.classList.add("active");
    }
    var defaultEdition = filterRoot.querySelector('[data-edition="all"]');
    if (defaultEdition && !filterRoot.querySelector("[data-edition].active")) {
      defaultEdition.classList.add("active");
    }

    apply();
  }

  if (document.readyState !== "loading") {
    init();
  } else {
    document.addEventListener("DOMContentLoaded", init);
  }

  if (window.document$ && typeof window.document$.subscribe === "function") {
    window.document$.subscribe(init);
  }
})();
