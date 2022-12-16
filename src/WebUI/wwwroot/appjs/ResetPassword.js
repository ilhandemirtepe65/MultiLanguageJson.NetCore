
$(document).ready(function () {
    $('#LanguageMenu').show();
    SetResetPassWordPreventReadAble();
    generate();
    const togglePassword = document.querySelector('#togglePassword');
    const password = document.querySelector('#Password');
    togglePassword.addEventListener('click', function (e) {
        const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
        password.setAttribute('type', type);
        this.classList.toggle('fa-eye-slash');
    });

    const currenttogglePassword = document.querySelector('#currenttogglePassword');
    const currentPassword = document.querySelector('#CurrentPassword');
    currenttogglePassword.addEventListener('click', function (e) {
        const type = currentPassword.getAttribute('type') === 'password' ? 'text' : 'password';
        currentPassword.setAttribute('type', type);
        this.classList.toggle('fa-eye-slash');
    });


    const togglepasswordconfirm = document.querySelector('#passwordConfirmtogglePassword');
    const passwordConfirm = document.querySelector('#PasswordConfirm');
    togglepasswordconfirm.addEventListener('click', function (e) {
        const type = passwordConfirm.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordConfirm.setAttribute('type', type);
        this.classList.toggle('fa-eye-slash');
    });
});

function generate()
{
    const captchinput = document.getElementById('generated-captcha');
    captchinput.addEventListener("mousedown", function (event) {
        event.preventDefault();
    });
    $.ajax({
        url: "generatecaptcha",
        typr: "GET",
        dataType: "text",
        success: function (response) {
            captchinput.value = response;
        },
        error: function (errormessage) {

        }
    });
    document.getElementById("entered-captcha").value = '';
}

$('#refresh').click(function (e) {
    generate();
});
$("#ResetPassWord").click(function () {
    $("#danger_alert_message").hide();
    if (ValidateCapture()) {
        Confirm();
    }
    else {
        if ($('#danger_alert_message').css('display') == 'none') {
            $("#danger_alert_message").show();
            $('#danger_alert_message').html("");
            $('#danger_alert_message').html($('#captchaMatchMessage').val());
        }
        generate();
    }
});

function Confirm() {
    $.ajax({
        cache: false,
        type: "POST",
        data: {
            UserName: $('#UserName').val(),
            CurrentPassword: $('#CurrentPassword').val(),
            Password: $('#Password').val(),
            PasswordConfirm: $('#PasswordConfirm').val(),
            Captcha: $('#generated-captcha').val(),
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        url: "Home/Insertpassword",
        dataType: "json",
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        success: function (result) {
            if (result) {
                OpenSmsPage();
            }
        },
        error: function (errormessage) {
            if (errormessage.responseJSON.errors.length > 0) {
                let text = "";
                $.each(errormessage.responseJSON.errors, function (index, val) {
                    text += val + "<br>";
                });
                if ($('#danger_alert_message').css('display') == 'none') {
                    $("#danger_alert_message").show();
                    $('#danger_alert_message').html("");
                    $('#danger_alert_message').html(text);
                }
                Clear();
                generate();
            }
        }
    });
}
function Clear() {
    $('#UserName').val("");
    $('#CurrentPassword').val("");
    $('#Password').val("");
    $('#PasswordConfirm').val("");
    generate();
}

function ValidateCapture() {
    let userValue = document.getElementById("entered-captcha").value.trim();
    let captcha = document.getElementById("generated-captcha").value.trim();
    if (userValue == captcha) {
        return true;
    }
    else {
        return false;
    }
}
function OpenSmsPage() {
    window.location.href = window.location.origin + window.location.pathname + "/resetsms";

}

var _username = false;
var _password = false;
var _newpassword = false;
var _newpasswordconfirm = false;
var _captcha = false;

$('input').each(function () {
    $(this).bind("keypress keyup keydown input propertychange", function () {
        var inputId = $(this).prop("id");
        var inputValue = $('#' + inputId + '').val();
        var status = inputValue.length > 0 ? true : false;
        if (inputId === "UserName") {
            _username = status
        }
        else if (inputId === "CurrentPassword") {
            _password = status
        }
        else if (inputId === "Password") {
            _newpassword = status
        }
        else if (inputId === "PasswordConfirm") {
            _newpasswordconfirm = status
        }
        if (inputId === "entered-captcha") {
            _captcha = status
        }
        SetResetPassWordPreventReadAble();
    })
});

function SetResetPassWordPreventReadAble() {
    if (_username && _password && _newpassword && _newpasswordconfirm && _captcha) {
        $("#ResetPassWord").prop("disabled", false);
    }
    else {
        $("#ResetPassWord").attr('disabled', 'disabled');
    }
}



