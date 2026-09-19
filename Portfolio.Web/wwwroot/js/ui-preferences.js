(() => {
    const body = document.body;
    const mode = (body?.dataset.themeMode || "system").toLowerCase();
    const darkModeEnabled = body?.dataset.darkModeEnabled !== "false";
    const storageKey = "portfolio-theme-mode";

    function resolveTheme(preferred) {
        if (preferred === "light" || preferred === "dark") return preferred;
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function applyTheme(preferred, persist = false) {
        if (!darkModeEnabled) {
            preferred = "light";
            localStorage.removeItem(storageKey);
        }

        const theme = resolveTheme(preferred);
        body?.setAttribute("data-theme", theme);
        if (persist) localStorage.setItem(storageKey, preferred);
        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            button.classList.toggle("is-on", theme === "dark");
            const text = button.querySelector(".theme-toggle-text");
            if (text) text.textContent = theme === "dark" ? "ON" : "OFF";
            button.setAttribute("aria-label", theme === "dark" ? "Switch to light mode" : "Switch to night mode");
            button.title = theme === "dark" ? "Switch to light mode" : "Switch to night mode";
        });
        const label = document.getElementById("themeModeLabel");
        if (label) label.textContent = preferred === "system" ? "System" : preferred.charAt(0).toUpperCase() + preferred.slice(1);
    }

    const saved = localStorage.getItem(storageKey);
    const initial = darkModeEnabled && (saved === "light" || saved === "dark" || saved === "system")
        ? saved
        : (darkModeEnabled ? mode : "light");

    if (!darkModeEnabled) {
        document.querySelectorAll("[data-theme-toggle]").forEach(button => button.remove());
    }

    applyTheme(initial, false);

    document.querySelectorAll("[data-theme-toggle]").forEach(button => {
        button.addEventListener("click", () => {
            const current = body?.getAttribute("data-theme") || resolveTheme(initial);
            const next = current === "dark" ? "light" : "dark";
            applyTheme(next, true);
        });
    });

    document.querySelector("[data-theme-reset]")?.addEventListener("click", () => {
        localStorage.removeItem(storageKey);
        applyTheme(mode, false);
    });

    const media = window.matchMedia("(prefers-color-scheme: dark)");
    media.addEventListener?.("change", () => {
        if (!darkModeEnabled) return;
        const current = localStorage.getItem(storageKey);
        if (!current || current === "system") applyTheme("system", false);
    });

    async function applyArabicTranslations() {
        if (document.documentElement.lang !== "ar") return;
        try {
            const response = await fetch("/localization/ui.ar.json", { cache: "no-store" });
            if (!response.ok) return;
            const map = await response.json();
            window.__uiTranslationMap = map;
            window.__uiTranslate = text => map[text] || text;
            const translateTextNode = node => {
                const value = node.nodeValue?.trim();
                if (!value || !map[value]) return;
                const leading = node.nodeValue.match(/^\s*/)?.[0] || "";
                const trailing = node.nodeValue.match(/\s*$/)?.[0] || "";
                node.nodeValue = `${leading}${map[value]}${trailing}`;
            };
            const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
                acceptNode(node) {
                    if (!node.parentElement || ["SCRIPT", "STYLE", "NOSCRIPT"].includes(node.parentElement.tagName)) return NodeFilter.FILTER_REJECT;
                    return NodeFilter.FILTER_ACCEPT;
                }
            });
            const nodes = [];
            let node;
            while ((node = walker.nextNode())) nodes.push(node);
            nodes.forEach(translateTextNode);

            document.querySelectorAll("[placeholder],[title],[aria-label]").forEach(el => {
                ["placeholder", "title", "aria-label"].forEach(attribute => {
                    const value = el.getAttribute(attribute);
                    if (value && map[value]) el.setAttribute(attribute, map[value]);
                });
            });
        } catch { /* English fallback remains intact. */ }
    }

    applyArabicTranslations();
})();
