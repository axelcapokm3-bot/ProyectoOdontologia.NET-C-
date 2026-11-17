document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("loginForm");
    const usuarioInput = document.getElementById("usuario");
    const passwordInput = document.getElementById("password");
    const passwordToggle = document.getElementById("passwordToggle");
    const submitButton = document.getElementById("loginSubmit");
    const errorBox = document.getElementById("loginError");
    const errorText = document.getElementById("loginErrorText");

    const rolesConAcceso = ["Admin", "Odontologo", "Recepcionista"];

    const setError = (message) => {
        if (!errorBox || !errorText) return;
        errorText.textContent = message || "";
        errorBox.classList.toggle("is-visible", Boolean(message));
    };

    const setLoading = (loading) => {
        if (!submitButton) return;
        submitButton.disabled = loading;
        submitButton.classList.toggle("is-loading", loading);
        submitButton.setAttribute("aria-busy", String(loading));
    };

    if (passwordToggle && passwordInput) {
        passwordToggle.addEventListener("click", () => {
            const mostrar = passwordInput.type === "password";
            passwordInput.type = mostrar ? "text" : "password";
            passwordToggle.setAttribute("aria-pressed", String(mostrar));
            passwordToggle.setAttribute("aria-label", mostrar ? "Ocultar contraseña" : "Mostrar contraseña");
            const icono = passwordToggle.querySelector(".material-symbols-outlined");
            if (icono) icono.textContent = mostrar ? "visibility_off" : "visibility";
            passwordInput.focus();
        });
    }

    if (!form) return;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        setError("");

        const nombreUsuario = usuarioInput?.value?.trim() || "";
        const password = passwordInput?.value || "";

        if (!nombreUsuario || !password) {
            setError("Completá tu usuario y contraseña para continuar.");
            (!nombreUsuario ? usuarioInput : passwordInput)?.focus();
            return;
        }

        setLoading(true);

        try {
            const auth = await Api.login({ nombreUsuario, password });

            if (!rolesConAcceso.includes(auth?.rol)) {
                Api.clearSession();
                setError("Esta cuenta no tiene acceso al panel clínico. Contactá al administrador.");
                return;
            }

            Api.setSession(auth);
            localStorage.setItem(APP_CONFIG.userKey, JSON.stringify({
                nombreUsuario: auth.nombreUsuario || nombreUsuario,
                email: auth.email || "",
                rol: auth.rol || ""
            }));

            document.body.classList.add("is-leaving");
            window.setTimeout(() => {
                window.location.href = "Api.html";
            }, 180);
        } catch (error) {
            const mensaje = error instanceof TypeError
                ? "No se pudo conectar con el servidor de la clínica. Verificá la URL de la API."
                : (error.message || "No se pudo iniciar sesión.");
            setError(mensaje);
        } finally {
            setLoading(false);
        }
    });
});
