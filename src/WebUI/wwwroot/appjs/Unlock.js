$(document).ready(function () {
    $('#LanguageMenu').show();
    SetUnlockPreventReadAble();
    generate();
});

function generate() {
   
    const captchinput = document.getElementById('generated-captcha');
    captchinput.addEventListener("mousedown", function (event) {
        event.preventDefault();
    });
    $.ajax({
        url: "generatecaptcha",
        typr: "GET",
        dataType: "text",
        success: function (response)
        {
            captchinput.value = response;
        },
        error: function (errormessage)
        {

        }
    });
    document.getElementById("entered-captcha").value = '';
}
$('#refresh').click(function (e) {
    generate();
});

function Unlock() {
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
}
function Confirm() {
    $.ajax({
        cache: false,
        type: "POST",
        data: {
            UserName: $('#UserName').val(),
            Captcha: $('#generated-captcha').val(),
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        url: "Home/Unlockpassword",
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
    window.location.href = window.location.origin + window.location.pathname + "/sms";
}

var _username = false;
var _captcha = false;
$('input').each(function () {
    $(this).bind("keypress keyup keydown input propertychange", function () {
        var inputId = $(this).attr("id");
        var inputValue = $('#' + inputId + '').val();
        var status = inputValue.length > 0 ? true : false;
        if (inputId === "UserName")
        {
            _username = status;
        }
        if (inputId === "entered-captcha") {
            _captcha = status;
        }
        SetUnlockPreventReadAble();
    })
});

function SetUnlockPreventReadAble() {
    if (_username && _captcha) {
        $("#UnlockButton").prop("disabled", false);
    }
    else {
        $("#UnlockButton").attr('disabled', 'disabled');
    }
}


