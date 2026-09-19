document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.getElementById("adminSidebar");
    const overlay = document.getElementById("sidebarOverlay");
    const menuToggle = document.getElementById("menuToggle");
    const closeButton = document.getElementById("sidebarClose");

    const openSidebar = () => {
        sidebar?.classList.add("open");
        overlay?.classList.add("show");
        document.body.style.overflow = "hidden";
    };

    const closeSidebar = () => {
        sidebar?.classList.remove("open");
        overlay?.classList.remove("show");
        document.body.style.overflow = "";
    };

    menuToggle?.addEventListener("click", openSidebar);
    closeButton?.addEventListener("click", closeSidebar);
    overlay?.addEventListener("click", closeSidebar);

    window.addEventListener("resize", () => {
        if (window.innerWidth > 900) closeSidebar();
    });

    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll(".sidebar-nav .nav-item").forEach((link) => {
        const href = (link.getAttribute("href") || "").toLowerCase();
        if (href && href !== "#" && currentPath.startsWith(href)) {
            link.classList.add("active");
        }
    });
});
