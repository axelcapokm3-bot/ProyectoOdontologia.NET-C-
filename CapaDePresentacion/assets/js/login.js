document.addEventListener("DOMContentLoaded", () => {
    const inputs = document.querySelectorAll('input[type="text"], input[type="password"]');
    inputs.forEach(input => {
        input.addEventListener("focus", (e) => {
            const label = e.target.previousElementSibling;
            if (label) label.classList.add("text-primary");
        });
        input.addEventListener("blur", (e) => {
            const label = e.target.previousElementSibling;
            if (label) label.classList.remove("text-primary");
        });
    });

    const loginForm = document.getElementById("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const nombreUsuario = document.getElementById("email")?.value?.trim();
            const password = document.getElementById("password")?.value ?? "";

            if (!nombreUsuario || !password) return;

            try {
                const auth = await Api.login({ nombreUsuario, password });
                Api.setSession(auth);
                window.location.href = "Api.html";
            } catch (error) {
                alert(error.message || "No se pudo iniciar sesión.");
            }
        });
    }
});
