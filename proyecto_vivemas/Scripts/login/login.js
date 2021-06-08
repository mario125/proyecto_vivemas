/*(function () {
    'use strict';
    window.addEventListener('load', function () {
        // Fetch all the forms we want to apply custom Bootstrap validation styles to
        var forms = document.getElementsByClassName('needs-validation');
        // Loop over them and prevent submission
        var validation = Array.prototype.filter.call(forms, function (form) {
            form.addEventListener('submit', function (event) {                
                if (form.checkValidity() === false) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }, false);
})();*/

//
$(document).ready(function () {
    $("#formLogin").validate({
        rules: {
            inputUsuario: {
                required: true
            },
            inputPassword: {
                required: true
            }
        }
    });
});

function validarForm(clase) {
    var forms = document.getElementsByClassName(clase);
    console.log(forms);
    var isValid;
    var validation = Array.prototype.filter.call(forms, function (form) {
        isValid = form.checkValidity();
    });
    return isValid;
}

const validarLogin = async () => {
    if ($("#formLogin").valid()) {        
        document.querySelector("#buttonEnviar").disabled = true;
        try {
            var data = new Object();
            data.usuario = document.querySelector("#inputUsuario").value;
            data.password = document.querySelector("#inputPassword").value;

            const response = await axios.post('/usuarios/AutenticarNuevo', JSON.stringify(data), {
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.data.flag === 0) {
                toastr["warning"]("usuario o contraseña incorrecto");
                document.querySelector("#buttonEnviar").disabled = false;
            } else if (response.data.flag === 1) {
                toastr["success"]("Usuario correcto");
                window.location.replace("/home/FrontPage")
            } else if (response.data.flag === 2) {
                toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
                document.querySelector("#buttonEnviar").disabled = false;
            } else if (response.data.flag === 3) {
                toastr["warning"]("usuario o contraseña incorrecto");
                document.querySelector("#buttonEnviar").disabled = false;
            }
        } catch (error) {
            console.log(error);
        }
    }
}

//function validarLogin() {  


/*if (validarForm('needs-validation')) {
    var data = new Object();
    data.usuario = document.querySelector("#inputUsuario").value;
    data.password = document.querySelector("#inputPassword").value;
    fetch("/usuarios/Autenticar", {
        method: 'POST',
        body: JSON.stringify(data),
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(res => res.json())
        .then(response => {
            if (response.flag === 1) {
                window.location.replace("/empresas/Detalles");
            } else if (response.flag == 2) {
                toastr["error"]("Hubo un error en la validacion");
            } else if (response.flag == 0) {
                toastr["warning"]("Usuario Incorrecto");
            }
        })
        .catch(error => console.log(error))
}   */

//}