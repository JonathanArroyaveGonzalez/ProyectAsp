document.addEventListener('DOMContentLoaded', function () {
    // Referencias a elementos DOM
    const form = document.querySelector('form[name="f1"]');
    const tablaBicicletas = document.getElementById('tablaBicicletas');
    const tbodyBicicletas = document.getElementById('tbodyBicicletas');
    const btnNuevo = document.getElementById('btnNuevo');
    const bicicletaModal = new bootstrap.Modal(document.getElementById('bicicletaModal'));
    const confirmarEliminarModal = new bootstrap.Modal(document.getElementById('confirmarEliminarModal'));
    const btnConfirmarEliminar = document.getElementById('btnConfirmarEliminar');

    let isSubmitting = false;
    let bicicletaIdParaEliminar = null;

    // Cargar datos al iniciar la página
    cargarBicicletas();

    // Event listeners
    if (form) {
        form.addEventListener('submit', handleSubmit);
    }

    if (btnNuevo) {
        btnNuevo.addEventListener('click', abrirModalNuevo);
    }

    if (btnConfirmarEliminar) {
        btnConfirmarEliminar.addEventListener('click', confirmarEliminarBicicleta);
    }

    // Funciones CRUD
    async function cargarBicicletas() {
        try {
            const response = await fetch('/Productos/ObtenerTodos', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (data.success) {
                renderizarTablaBicicletas(data.data);
            } else {
                mostrarMensaje(data.mensaje || 'Error al cargar bicicletas', 'error');
            }
        } catch (error) {
            console.error("Error al cargar bicicletas:", error);
            mostrarMensaje('Error al comunicarse con el servidor', 'error');
        }
    }

    function renderizarTablaBicicletas(bicicletas) {
        if (!tbodyBicicletas) return;

        tbodyBicicletas.innerHTML = '';

        if (bicicletas.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = '<td colspan="3" class="text-center">No hay bicicletas registradas</td>';
            tbodyBicicletas.appendChild(row);
            return;
        }

        bicicletas.forEach(bicicleta => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${bicicleta.placa}</td>
                <td>${bicicleta.marca}</td>
                <td>
                    <button class="btn btn-sm btn-primary btn-editar" data-id="${bicicleta.id}">Editar</button>
                    <button class="btn btn-sm btn-danger btn-eliminar" data-id="${bicicleta.id}">Eliminar</button>
                </td>
            `;
            tbodyBicicletas.appendChild(row);

            // Agregar event listeners a los botones
            row.querySelector('.btn-editar').addEventListener('click', () => editarBicicleta(bicicleta.id));
            row.querySelector('.btn-eliminar').addEventListener('click', () => mostrarConfirmacionEliminar(bicicleta.id));
        });
    }

    function abrirModalNuevo() {
        document.getElementById('bicicletaModalLabel').textContent = 'Nueva Bicicleta';
        document.getElementById('operacion').value = 'insertar';
        document.getElementById('idBicicleta').value = '';
        document.getElementById('Placa').value = '';
        document.getElementById('Marca').value = '';
        bicicletaModal.show();
    }

    async function editarBicicleta(id) {
        try {
            const response = await fetch(`/Productos/ObtenerPorId?id=${id}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (data.success) {
                document.getElementById('bicicletaModalLabel').textContent = 'Editar Bicicleta';
                document.getElementById('operacion').value = 'actualizar';
                document.getElementById('idBicicleta').value = data.data.id;
                document.getElementById('Placa').value = data.data.placa;
                document.getElementById('Marca').value = data.data.marca;
                bicicletaModal.show();
            } else {
                mostrarMensaje(data.mensaje || 'Error al cargar datos de la bicicleta', 'error');
            }
        } catch (error) {
            console.error("Error al cargar datos para editar:", error);
            mostrarMensaje('Error al comunicarse con el servidor', 'error');
        }
    }

    function mostrarConfirmacionEliminar(id) {
        bicicletaIdParaEliminar = id;
        confirmarEliminarModal.show();
    }

    async function confirmarEliminarBicicleta() {
        if (!bicicletaIdParaEliminar) return;

        try {
            const response = await fetch('/Productos/Eliminar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(bicicletaIdParaEliminar)
            });

            const data = await response.json();

            confirmarEliminarModal.hide();

            if (data.success) {
                mostrarMensaje(data.mensaje, 'success');
                cargarBicicletas();
            } else {
                mostrarMensaje(data.mensaje || 'Error al eliminar la bicicleta', 'error');
            }
        } catch (error) {
            confirmarEliminarModal.hide();
            console.error("Error al eliminar:", error);
            mostrarMensaje('Error al comunicarse con el servidor', 'error');
        } finally {
            bicicletaIdParaEliminar = null;
        }
    }

    async function handleSubmit(event) {
        event.preventDefault();
        event.stopImmediatePropagation();

        if (isSubmitting) return;
        isSubmitting = true;

        const submitButton = form.querySelector('button[type="submit"]');
        const originalButtonText = submitButton.textContent;
        submitButton.disabled = true;
        submitButton.textContent = 'Guardando...';

        try {
            const formData = {
                Id: parseInt(document.getElementById('idBicicleta').value) || 0,
                Placa: document.getElementById('Placa').value.trim(),
                Marca: document.getElementById('Marca').value.trim()
            };

            if (!formData.Placa || !formData.Marca) {
                mostrarMensaje("Por favor complete todos los campos", 'error');
                return;
            }

            const response = await fetch('/Productos/Guardar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.mensaje || 'Error en la solicitud');
            }

            if (data.errors) {
                mostrarMensaje(`Errores:\n${data.errors.join('\n')}`, 'error');
            } else {
                mostrarMensaje(data.mensaje, 'success');
                if (data.success) {
                    form.reset();
                    bicicletaModal.hide();
                    cargarBicicletas();

                    // Añade esta línea para asegurarte de que el modal se cierra completamente
                    document.querySelector('.modal-backdrop').remove();
                    document.body.classList.remove('modal-open');
                    document.body.style.overflow = '';
                    document.body.style.paddingRight = '';
                }
            }
        } catch (error) {
            console.error("Error:", error);
            mostrarMensaje(error.message || "Error al procesar la solicitud", 'error');
        } finally {
            isSubmitting = false;
            submitButton.disabled = false;
            submitButton.textContent = originalButtonText;
        }
    }

    function mostrarMensaje(mensaje, tipo = 'info') {
        // Puedes implementar esto con bootstrap toasts, sweet alert, etc.
        alert(mensaje);
    }
});