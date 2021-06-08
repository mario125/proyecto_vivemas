function validarForm(forms) {    
    //var forms = document.getElementsByClassName(clase);
    var isValid;
    var validation = Array.prototype.filter.call(forms, function (form) {       
        isValid = form.checkValidity();
        form.classList.add('was-validated');
    });
    return isValid;
}

jQuery.validator.setDefaults({
    onkeyup: false,
    errorPlacement: function (error, element) {
        error.addClass('invalid-feedback');
        element.closest('.form-group').append(error);
    },
    highlight: function (element, errorClass, validClass) {
        $(element).removeClass('is-valid').addClass('is-invalid');
    },
    unhighlight: function (element, errorClass, validClass) {
        $(element).removeClass('is-invalid').addClass('is-valid');
    }
});

Number.prototype.pad = function (size) {
    var s = String(this);
    while (s.length < (size || 2)) { s = "0" + s; }
    return s;
}