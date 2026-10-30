// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll("[data-copy-value]").forEach((button) => {
    button.addEventListener("click", async () => {
        const value = button.dataset.copyValue ?? "";
        const feedback = document.getElementById(button.dataset.copyFeedback ?? "");
        let copied = false;

        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(value);
                copied = true;
            } else {
                const temporary = document.createElement("textarea");
                temporary.value = value;
                temporary.setAttribute("readonly", "");
                temporary.style.position = "fixed";
                temporary.style.opacity = "0";
                document.body.appendChild(temporary);
                temporary.select();
                copied = document.execCommand("copy");
                temporary.remove();
            }
        } catch {
            copied = false;
        }

        if (feedback) feedback.textContent = copied ? "Código copiado. Ya puedes compartirlo." : "No se pudo copiar; selecciona el código y cópialo.";
        if (copied) {
            const original = button.textContent;
            button.textContent = "Copiado";
            window.setTimeout(() => { button.textContent = original; }, 1800);
        }
    });
});
