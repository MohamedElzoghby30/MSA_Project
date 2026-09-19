document.addEventListener("DOMContentLoaded", () => {
    document.body.classList.add("js-enabled");

    const animationsEnabled = getComputedStyle(document.body)
        .getPropertyValue("--animations")
        .trim() !== "0";

    const revealItems = document.querySelectorAll(".reveal");

    revealItems.forEach((element) => {
        const duration = Number(element.dataset.duration || 700);
        const delay = Number(element.dataset.delay || 0);
        element.style.setProperty("--reveal-duration", `${duration}ms`);

        const productCard = element.closest(".products-showcase")
            ? element.classList.contains("product-card")
            : false;

        if (productCard) {
            const index = [...element.parentElement.children].indexOf(element);
            element.style.transitionDelay = `${delay + Math.max(0, index) * 110}ms`;
        } else {
            element.style.transitionDelay = `${delay}ms`;
        }
    });

    if (!animationsEnabled || window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        revealItems.forEach((element) => element.classList.add("in-view"));
    } else {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                entry.target.classList.add("in-view");
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.12, rootMargin: "0px 0px -35px 0px" });

        revealItems.forEach((element) => observer.observe(element));
    }

    const mobileMenuButton = document.getElementById("mobileMenuBtn");
    const mainNav = document.getElementById("mainNav");

    mobileMenuButton?.addEventListener("click", () => {
        const open = mainNav.classList.toggle("open");
        mobileMenuButton.setAttribute("aria-expanded", String(open));
    });

    document.querySelectorAll("#mainNav a").forEach((link) => {
        link.addEventListener("click", () => mainNav.classList.remove("open"));
    });

    document.querySelectorAll("[data-custom-select]").forEach((select) => {
        const hidden = select.querySelector('input[type="hidden"]');
        const trigger = select.querySelector(".custom-select-trigger");
        const triggerText = trigger?.querySelector("span:first-child");
        const options = select.querySelectorAll(".custom-select-menu button[data-value]");

        trigger?.addEventListener("click", (event) => {
            event.preventDefault();

            document.querySelectorAll("[data-custom-select].open").forEach((other) => {
                if (other !== select) other.classList.remove("open");
            });

            select.classList.toggle("open");
            trigger.setAttribute(
                "aria-expanded",
                String(select.classList.contains("open"))
            );
        });

        options.forEach((option) => {
            option.addEventListener("click", () => {
                const value = option.dataset.value || "";

                if (hidden) {
                    hidden.value = value;
                }

                if (triggerText) {
                    triggerText.textContent = window.__uiTranslate
                        ? window.__uiTranslate(value)
                        : value;
                }

                select.classList.remove("open");
                trigger?.setAttribute("aria-expanded", "false");
            });
        });
    });
    // =====================================================
    // Continuous Product Cards Animation
    // Card 1 -> Card 2 -> Card 3 -> ... -> Card 1
    // New card starts every 3 seconds
    // =====================================================

    const productCards = [
        ...document.querySelectorAll(".products-showcase .product-card")
    ];

    if (
        productCards.length > 0 &&
        animationsEnabled &&
        !window.matchMedia("(prefers-reduced-motion: reduce)").matches
    ) {
        const liftDuration = 950;
        const animationInterval = 3000;

        const sleep = (ms) =>
            new Promise((resolve) => setTimeout(resolve, ms));

        const runProductsForever = async () => {
            let index = 0;

            while (true) {
                const card = productCards[index];

                if (!card.classList.contains("in-view")) {
                    await sleep(200);
                    continue;
                }

                card.classList.add("product-lift");

                await sleep(liftDuration);

                card.classList.remove("product-lift");

                await sleep(animationInterval - liftDuration);

                index++;

                if (index >= productCards.length) {
                    index = 0;
                }
            }
        };

        runProductsForever();
    }

    // =====================================================
    // Hero Title Typewriter Animation
    // Type -> Wait -> Delete -> Repeat
    // =====================================================

    const heroTitle = document.querySelector(".hero-copy h1");

    if (
        heroTitle &&
        animationsEnabled &&
        !window.matchMedia("(prefers-reduced-motion: reduce)").matches
    ) {
        const originalText = heroTitle.textContent.trim();

        let typingIndex = 0;
        let deletingIndex = originalText.length;
        let isDeleting = false;

        heroTitle.textContent = "";

        const cursor = document.createElement("span");
        cursor.className = "hero-type-cursor";
        cursor.textContent = "|";

        heroTitle.appendChild(cursor);

        const typeWriter = () => {
            if (!isDeleting) {
                if (typingIndex < originalText.length) {
                    cursor.before(originalText.charAt(typingIndex));
                    typingIndex++;

                    setTimeout(typeWriter, 45);
                } else {
                    setTimeout(() => {
                        isDeleting = true;
                        deletingIndex = originalText.length;
                        typeWriter();
                    }, 700);
                }
            } else {
                if (deletingIndex > 0) {
                    const lastTextNode = [...heroTitle.childNodes]
                        .filter(node => node.nodeType === Node.TEXT_NODE)
                        .pop();

                    if (lastTextNode) {
                        lastTextNode.textContent =
                            lastTextNode.textContent.slice(0, -1);
                    }

                    deletingIndex--;

                    [...heroTitle.childNodes].forEach(node => {
                        if (
                            node.nodeType === Node.TEXT_NODE &&
                            node.textContent === ""
                        ) {
                            node.remove();
                        }
                    });

                    setTimeout(typeWriter, 25);
                } else {
                    isDeleting = false;
                    typingIndex = 0;

                    setTimeout(typeWriter, 300);
                }
            }
        };

        typeWriter();
    }

    document.addEventListener("click", (event) => {
        const target = event.target;

        document.querySelectorAll("[data-custom-select].open").forEach((select) => {
            if (!(target instanceof Node) || !select.contains(target)) {
                select.classList.remove("open");
                select
                    .querySelector(".custom-select-trigger")
                    ?.setAttribute("aria-expanded", "false");
            }
        });
    });

    // =====================================================
    // Image Lightbox
    // Click image -> beautiful zoom animation
    // =====================================================

    const lightboxImages = document.querySelectorAll(
        ".products-showcase .product-card .card-media img"
    );

    if (lightboxImages.length > 0) {

        const lightbox = document.createElement("div");
        lightbox.className = "image-lightbox";

        lightbox.innerHTML = `
            <div class="image-lightbox-backdrop"></div>

            <button
                type="button"
                class="image-lightbox-close"
                aria-label="Close image">
                ×
            </button>

            <div class="image-lightbox-content">
                <img src="" alt="" />
            </div>
        `;

        document.body.appendChild(lightbox);

        const lightboxImage = lightbox.querySelector(
            ".image-lightbox-content img"
        );

        const closeButton = lightbox.querySelector(
            ".image-lightbox-close"
        );

        const backdrop = lightbox.querySelector(
            ".image-lightbox-backdrop"
        );

        const openLightbox = (image) => {

            lightboxImage.src = image.currentSrc || image.src;
            lightboxImage.alt = image.alt || "";

            lightbox.classList.remove("closing");

            document.body.classList.add("lightbox-open");

            requestAnimationFrame(() => {
                lightbox.classList.add("is-open");
            });
        };

        const closeLightbox = () => {

            lightbox.classList.remove("is-open");
            lightbox.classList.add("closing");

            setTimeout(() => {
                lightbox.classList.remove("closing");
                document.body.classList.remove("lightbox-open");
                lightboxImage.src = "";
            }, 350);
        };

        lightboxImages.forEach((image) => {

            image.classList.add("lightbox-trigger");

            image.addEventListener("click", () => {
                openLightbox(image);
            });
        });

        closeButton.addEventListener("click", closeLightbox);

        backdrop.addEventListener("click", closeLightbox);

        lightbox.querySelector(
            ".image-lightbox-content"
        ).addEventListener("click", (event) => {
            event.stopPropagation();
        });

        document.addEventListener("keydown", (event) => {

            if (event.key === "Escape" &&
                lightbox.classList.contains("is-open")) {

                closeLightbox();
            }
        });
    }
});
