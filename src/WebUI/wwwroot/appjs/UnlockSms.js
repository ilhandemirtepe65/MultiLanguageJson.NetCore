$(document).ready(function () {
    $('#LanguageMenu').hide();
    var count = parseInt($('#unlocksmsTimeOut').val());
    var timer = document.getElementById("timer");
    if (count > 0) {
        function TimerCount() {
            if (count > 0) {
                count--;
                timer.innerHTML = $('#timeText1').val() + " " + count + " " + $('#timeText2').val();
                $('#timerCount').val(count);
            }
            else {
                window.location.href = window.location.origin + window.location.pathname + "/unlock";
            }
        }
    }
    setInterval(TimerCount, 1000);
    SetSmsReadAble();
});
function SendSms() {
    $("#danger_alert_message").hide();
    $.ajax({
        cache: false,
        type: "POST",
        data: {
            SmsCode: $('#SmsCode').val(),
            RemainTime: $('#timerCount').val(),
            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        },
        url: "Home/UnlockPasswordSendSms",
        dataType: "json",
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        success: function (result) {
            if (result) {
                window.location.href = window.location.origin + window.location.pathname + "/unlock?sucessmessage=true";

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
            }
        }
    });
}
function SetSmsReadAble()
{
    $("#sendSmsToTelephone").attr('disabled', 'disabled');
}

$("#SmsCode").bind("keypress keyup keydown input propertychange", function ()
{
    if ($(this).val().length > 0)
    {
        $("#sendSmsToTelephone").prop("disabled", false);
    }
    else
    {
        SetSmsReadAble();
    }
});