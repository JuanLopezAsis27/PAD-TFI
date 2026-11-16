// Funciones del portal que no se pueden hacer desde Blazor:
//  - arcaSim.mostrarAviso: la usa Components/Avisos/Aviso.razor para traer a la vista un mensaje nuevo.
//  - Botones "Copiar" y pestañas de la página de documentación (/documentacion), que se renderiza
//    sin interactividad: se atienden acá, con un solo listener para toda la página.
(() => {
    const SEPARACION = 16; // px entre el aviso y el borde de la pantalla

    // Lo que tapa la parte de arriba de la pantalla: el encabezado fijo del sitio público.
    function altoEncabezadoFijo() {
        const encabezado = document.querySelector('[data-encabezado-fijo]');
        if (!encabezado) return 0;
        return Math.max(0, encabezado.getBoundingClientRect().bottom);
    }

    // Le pasa el foco al aviso (así los lectores de pantalla lo leen y quien usa el teclado sigue desde
    // ahí) y, si no se ve entero, desplaza la página hasta él.
    function mostrarAviso(elemento) {
        if (!elemento || !elemento.isConnected) return;

        elemento.focus({ preventScroll: true });

        // Se espera al próximo cuadro: el aviso se acaba de insertar y el navegador todavía tiene que
        // acomodar la página (scroll anchoring). Si se desplaza antes, ese ajuste corta el desplazamiento.
        requestAnimationFrame(() => requestAnimationFrame(() => desplazarHasta(elemento)));
    }

    function desplazarHasta(elemento) {
        if (!elemento.isConnected) return;

        const arriba = altoEncabezadoFijo();
        const abajo = window.innerHeight || document.documentElement.clientHeight;
        const caja = elemento.getBoundingClientRect();
        const seVeEntero = caja.top >= arriba && caja.bottom <= abajo;

        if (!seVeEntero) {
            const entra = caja.height <= abajo - arriba - 2 * SEPARACION;
            const sinMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            elemento.style.scrollMarginTop = `${arriba + SEPARACION}px`;
            elemento.style.scrollMarginBottom = `${SEPARACION}px`;
            elemento.scrollIntoView({
                behavior: sinMovimiento ? 'auto' : 'smooth',
                block: entra ? 'center' : 'start'
            });
        }
    }

    async function copiar(boton) {
        const codigo = boton.closest('[data-bloque-codigo]')?.querySelector('code');
        if (!codigo) return;

        const texto = codigo.textContent.trim();
        const original = boton.dataset.textoOriginal ?? boton.innerHTML;
        boton.dataset.textoOriginal = original;

        try {
            await navigator.clipboard.writeText(texto);
            boton.innerHTML = '<i class="bi bi-check2"></i> Copiado';
        } catch {
            // Sin acceso al portapapeles (por ejemplo, fuera de HTTPS): se selecciona para Ctrl+C.
            const rango = document.createRange();
            rango.selectNodeContents(codigo);
            const seleccion = window.getSelection();
            seleccion.removeAllRanges();
            seleccion.addRange(rango);
            boton.innerHTML = 'Seleccionado: Ctrl+C';
        }

        clearTimeout(boton._temporizador);
        boton._temporizador = setTimeout(() => { boton.innerHTML = original; }, 2000);
    }

    function elegirPestana(pestana) {
        const grupo = pestana.closest('[data-pestanas]');
        if (!grupo) return;

        const nombre = pestana.dataset.pestana;
        grupo.querySelectorAll('[data-pestana]').forEach(p => {
            const activa = p.dataset.pestana === nombre;
            p.classList.toggle('active', activa);
            p.setAttribute('aria-selected', String(activa));
        });
        grupo.querySelectorAll('[data-panel]').forEach(panel => {
            panel.hidden = panel.dataset.panel !== nombre;
        });
    }

    document.addEventListener('click', evento => {
        const boton = evento.target.closest('[data-copiar]');
        if (boton) {
            copiar(boton);
            return;
        }

        const pestana = evento.target.closest('[data-pestana]');
        if (pestana) elegirPestana(pestana);
    });

    window.arcaSim = { mostrarAviso };
})();
