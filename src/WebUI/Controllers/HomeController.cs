

namespace WebUI.Controllers;
public class HomeController : Controller
{
    private readonly IOptions<Settings> _options;
    private readonly ILogger<HomeController> _logger;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer<HomeController> _languageService;
    public HomeController(IOptions<Settings> options,
                          ILogger<HomeController> logger,
                          IUserService userService,
                          IHttpContextAccessor httpContextAccessor,
                          IStringLocalizer<HomeController> stringLocalizer)
    {
        _options = options;
        _logger = logger;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _languageService = stringLocalizer;
    }

    public IActionResult ChangeLanguage(string culture)
    {
        SetBrowserLanguage(culture);
        return Redirect(Request.Headers["Referer"].ToString());
    }
    [NonAction]
    public void SetBrowserLanguage(string culture)
    {
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
           CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
           new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddYears(1) });
    }
    public IActionResult ResetPassword(string sucessmessage = "")
    {

        string culture = _userService.GetBrowserLanguage();
        var filePath = $"Resources/{culture}.json";
        bool curl = System.IO.File.Exists(filePath) ? true : false;
        if (!curl)
        {
            culture = "tr-TR";
        }
        SetBrowserLanguage(culture);
        DisableSucessMessageForUnLock();
        if (string.IsNullOrEmpty(sucessmessage))
        {
            DisableSucessMessageForResetPassword();
        }
        else
        {
            EnableSucessMessageForResetPassword();
        }
        return View();
    }

    public IActionResult Unlock(string sucessmessage = "")
    {
        DisableSucessMessageForResetPassword();
        if (string.IsNullOrEmpty(sucessmessage))
        {
            DisableSucessMessageForUnLock();
        }
        else
        {
            EnableSucessMessageForUnLock();
        }
        return View();
    }

    public IActionResult ResetSms()
    {
        DisableSucessMessageForResetPassword();
        DisableSucessMessageForUnLock();
        string result = "";
        string telephoneNumber = _httpContextAccessor.HttpContext?.Session.GetString("telephoneNumber") ?? string.Empty;
        if (!string.IsNullOrEmpty(telephoneNumber))
        {
            result += _userService.GetPhoneFormat(telephoneNumber);
            result += " " + _languageService["telephoneMessage"].ToString();
            return View(model: result);
        }
        else
        {
            return RedirectToAction("ResetPassword", "Home");
        }
    }
    public IActionResult UnlockSms()
    {
        DisableSucessMessageForResetPassword();
        DisableSucessMessageForUnLock();
        string result = "";
        string telephoneNumber = _httpContextAccessor.HttpContext?.Session.GetString("unlock_telephoneNumber") ?? string.Empty;
        if (!string.IsNullOrEmpty(telephoneNumber))
        {
            result += _userService.GetPhoneFormat(telephoneNumber);
            result += " " + _languageService["telephoneMessage"].ToString();
            return View(model: result);
        }
        else
        {
            return RedirectToAction("Unlock", "Home");
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Insertpassword(UserViewModel _userViewModel, [FromServices] IValidator<UserViewModel> _validator, SmsCompanyFactory _smsCompanyFactory)
    {
        List<string> errorMessage = new List<string>();
        var resultvalidation = _validator.Validate(_userViewModel);
        if (resultvalidation.IsValid)
        {

            string gkod = _userViewModel.Captcha;
            bool isValid = false;
            string unameEncryption = PasswordReset.EncryptString(_options.Value.AesKey, _userViewModel.UserName);
            string newpasswordEncryption = PasswordReset.EncryptString(_options.Value.AesKey, _userViewModel.Password);
            string newpasswordconfirmEncryption = PasswordReset.EncryptString(_options.Value.AesKey, _userViewModel.PasswordConfirm);
            string currentpasswordEncryption = PasswordReset.EncryptString(_options.Value.AesKey, _userViewModel.CurrentPassword);
            string currentpasswordDecryption = PasswordReset.DecryptString(_options.Value.AesKey, currentpasswordEncryption);
            string newpasswordDecryption = PasswordReset.DecryptString(_options.Value.AesKey, newpasswordEncryption);
            string unameDecryption = PasswordReset.DecryptString(_options.Value.AesKey, unameEncryption);
            if (unameDecryption.Equals(_options.Value.ServiceAccount))
            {
                _logger.LogError($"Service hesabı şifresi değiştirilmeye çalışıldı.{unameDecryption}");
                errorMessage.Add(_languageService["userNotFound"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }

            if (currentpasswordDecryption == newpasswordDecryption)
            {
                _logger.LogError($"New password and old password same.{unameDecryption}");
                errorMessage.Add(_languageService["newAndOldPasswordSame"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            if (!_userService.DoesUserExist(unameDecryption, _options.Value.ADKey))
            {
                _logger.LogError($"User not found.{unameDecryption}");
                errorMessage.Add(_languageService["userNotFoundInStore"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }

            if (_userService.GetUserInfoCountPassWord(unameDecryption) > Convert.ToInt32(_options.Value.VTryCount))
            {
                TimeSpan timedifference = DateTime.Now - _userService.GetUserLoginTimeForPassWord(unameDecryption);
                _logger.LogError($"User mail not found.{unameDecryption}");
                errorMessage.Add(_languageService["multiTryCountMesssage"].ToString() + " " + timedifference.TotalMinutes.ToString() + " " + _languageService["afterThenMinute"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            DateTime dateTime = DateTime.Now.AddDays(-1);
            List<UserInfo> dailyHistory = _userService.GetUserPasswordDailyHistory(unameDecryption, dateTime);
            int count = dailyHistory.Count();
            if (count > Convert.ToInt32(_options.Value.DailyPasswordChanceCount))
            {
                _logger.LogError($"number of password changes per day {_options.Value.DailyPasswordChanceCount} exceeded.{unameDecryption}");
                errorMessage.Add(String.Format("{0} {1} {2}", _languageService["oneDayOncePasswordChanceCounttxt1"].ToString(), _options.Value.DailyPasswordChanceCount, _languageService["oneDayOncePasswordChanceCounttxt2"].ToString()));
            }
            PrincipalContext context = _userService.GetContext(_options.Value.ADKey);
            if (context != null)
            {
                isValid = context.ValidateCredentials(unameDecryption, currentpasswordDecryption);
                _logger.LogError($"Info {isValid.ToString()}");
            }
            if (!isValid)
            {
                _logger.LogError($"Current Password wrong.{unameDecryption}");
                errorMessage.Add(_languageService["currentPasswordError"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }

            var smsProvider = _smsCompanyFactory.ProduceSmsCompany(CompanyType.Custom);
            string telephoneNumber = _userService.GetMobilePhone(unameDecryption);
            string smsCode = _userService.CreateSmsCode();
            bool sendsms = false;
            sendsms = smsProvider.SendSms(smsCode, telephoneNumber);
            if (!sendsms)
            {
                _logger.LogError($"sms does not sent.{telephoneNumber}");
                errorMessage.Add(_languageService["smsErrorMessage"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            _httpContextAccessor.HttpContext?.Session.Remove("username");
            _httpContextAccessor.HttpContext?.Session.Remove("password");
            _httpContextAccessor.HttpContext?.Session.Remove("smsCode");
            RemoveSession_ResetPasswordTelephoneNumber();
            _httpContextAccessor.HttpContext?.Session.SetString("username", unameDecryption);
            _httpContextAccessor.HttpContext?.Session.SetString("password", newpasswordDecryption);
            _httpContextAccessor.HttpContext?.Session.SetString("smsCode", smsCode);
            _httpContextAccessor.HttpContext?.Session.SetString("telephoneNumber", telephoneNumber);
            return Json(1);
        }
        RemoveSession_ResetPasswordTelephoneNumber();
        errorMessage = resultvalidation.Errors.Select(x => x.ErrorMessage).ToList();
        return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
    }

    [NonAction]
    public void RemoveSession_ResetPasswordTelephoneNumber()
    {
        _httpContextAccessor.HttpContext?.Session.Remove("telephoneNumber");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Unlockpassword(UnlockViewModel _unlockViewModel, [FromServices] IValidator<UnlockViewModel> _validator, SmsCompanyFactory _smsCompanyFactory)
    {
        List<string> errorMessage = new List<string>();
        var resultvalidation = _validator.Validate(_unlockViewModel);
        if (resultvalidation.IsValid)
        {
            string unameEncryption = PasswordReset.EncryptString(_options.Value.AesKey, _unlockViewModel.UserName);
            string unameDecryption = PasswordReset.DecryptString(_options.Value.AesKey, unameEncryption);
            string gkod = _unlockViewModel.Captcha;
            if (unameDecryption.Equals(_options.Value.ServiceAccount))
            {
                _logger.LogError($"Service hesabı kilidi kaldırılmaya çalışıldı.{unameDecryption}");
                errorMessage.Add(_languageService["userNotFound"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            if (!_userService.DoesUserExist(unameDecryption, _options.Value.ADKey))
            {
                _logger.LogError($"User not found");
                errorMessage.Add(_languageService["userNotFoundInStore"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            if (_userService.GetUserInfoCountUnlock(unameDecryption) > Convert.ToInt32(_options.Value.VTryCount))
            {
                TimeSpan timedifference = DateTime.Now - _userService.GetUserLoginTimeForUnlock(unameDecryption);
                _logger.LogError($"Password reset limit exceeded.{unameDecryption}");
                errorMessage.Add(_languageService["multiTryCountMesssage"].ToString() + " " + timedifference.TotalMinutes.ToString() + " " + _languageService["afterThenMinute"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            //eğer hesap kilitli değilse sms gelmesin 
            PrincipalContext context = _userService.GetContext(_options.Value.ADKey);
            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, unameDecryption);
            if (!user.IsAccountLockedOut())
            {
                _logger.LogError($"User account is not locked..{unameDecryption}");
                errorMessage.Add(_languageService["noUnlock"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }

            var smsProvider = _smsCompanyFactory.ProduceSmsCompany(CompanyType.Custom);
            string telephoneNumber = _userService.GetMobilePhone(unameDecryption);
            string smsCode = _userService.CreateSmsCode();
            bool sendsms = false;
            sendsms = smsProvider.SendSms(smsCode, telephoneNumber);
            if (!sendsms)
            {
                _logger.LogError($"sms does not sent.{telephoneNumber}");
                errorMessage.Add(_languageService["smsErrorMessage"].ToString());
                return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
            }
            _httpContextAccessor.HttpContext?.Session.Remove("unlock_username");
            _httpContextAccessor.HttpContext?.Session.Remove("unlcok_smscode");
            RemoveSession_UnlockTelephoneNumber();
            _httpContextAccessor.HttpContext?.Session.SetString("unlock_username", unameDecryption);
            _httpContextAccessor.HttpContext?.Session.SetString("unlcok_smscode", smsCode);
            _httpContextAccessor.HttpContext?.Session.SetString("unlock_telephoneNumber", telephoneNumber);
            return Json(1);
        }
        RemoveSession_UnlockTelephoneNumber();
        errorMessage = resultvalidation.Errors.Select(x => x.ErrorMessage).ToList();
        return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
    }
    public void RemoveSession_UnlockTelephoneNumber()
    {
        _httpContextAccessor.HttpContext?.Session.Remove("unlock_telephoneNumber");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPasswordSendSms(SmsViewModel _smsViewModel, [FromServices] IValidator<SmsViewModel> validator)
    {
        List<string> errorMessage = new List<string>();
        var resultvalidation = validator.Validate(_smsViewModel);
        if (resultvalidation.IsValid)
        {
            string verificationTime = _smsViewModel.RemainTime;
            string username = _httpContextAccessor.HttpContext?.Session.GetString("username") ?? string.Empty;
            string newPassword = _httpContextAccessor.HttpContext?.Session.GetString("password") ?? string.Empty;
            string smsCode = _httpContextAccessor.HttpContext?.Session.GetString("smsCode") ?? string.Empty;
            int errorTryCount = _httpContextAccessor.HttpContext?.Session.GetInt32("errorTryCount") ?? 0;
            string verificationCode = _smsViewModel.SmsCode;

            if (verificationCode == smsCode)
            {
                _userService.SetUserInfoPassWordProcessDate(username);
                SetPassword(_userService.GetContext(_options.Value.ADKey), _options, username, newPassword);
                _httpContextAccessor.HttpContext?.Session.Remove("username");
                _httpContextAccessor.HttpContext?.Session.Remove("password");
                _httpContextAccessor.HttpContext?.Session.Remove("smsCode");
                _httpContextAccessor.HttpContext?.Session.Remove("errorTryCount");
                _logger.LogError($"Password reseted successfully");
                //alttaki session mesajı success mesajı ekranda gostermeye yarar ve mesajda session ile taşıdım
                EnableSucessMessageForResetPassword();
                _httpContextAccessor.HttpContext?.Session.SetString("Session_success_alert_message_Content", _languageService["passwordSetSucessfullyMessage"].ToString());

                return Json(1);
            }
            else
            {
                if (errorTryCount == 0)
                {
                    _httpContextAccessor.HttpContext?.Session.SetInt32("errorTryCount", 1);
                }
                else
                {
                    _httpContextAccessor.HttpContext?.Session.SetInt32("errorTryCount", errorTryCount + 1);
                }
                if (errorTryCount < 3)
                {
                    _logger.LogError($"Sms verification code is not correct");
                    errorMessage.Add(_languageService["smsVerificationError"].ToString());
                    return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
                }
                else
                {
                    _logger.LogError($"Sms verification code is not correct for 3 times, session cleared");
                    errorMessage.Add(_languageService["repeatedSmsCodeMessage"].ToString());
                    return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
                }
            }
        }
        errorMessage = resultvalidation.Errors.Select(x => x.ErrorMessage).ToList();
        return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UnlockPasswordSendSms(SmsViewModel _smsViewModel, [FromServices] IValidator<SmsViewModel> validator)
    {
        List<string> errorMessage = new List<string>();
        var resultvalidation = validator.Validate(_smsViewModel);
        if (resultvalidation.IsValid)
        {
            string verificationTime = _smsViewModel.RemainTime;
            string username = _httpContextAccessor.HttpContext?.Session.GetString("unlock_username") ?? string.Empty;
            string smsCode = _httpContextAccessor.HttpContext?.Session.GetString("unlcok_smscode") ?? string.Empty;
            int errorTryCount = _httpContextAccessor.HttpContext?.Session.GetInt32("unlock_errorTryCount") ?? 0;
            string verificationCode = _smsViewModel.SmsCode;

            if (verificationCode == smsCode)
            {
                PrincipalContext context = _userService.GetContext(_options.Value.ADKey);
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                if (!user.IsAccountLockedOut())
                {
                    _logger.LogError($"User account is not locked..{username}");
                    errorMessage.Add(_languageService["noUnlock"].ToString());
                    return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
                }
                else
                {
                    user.UnlockAccount();
                    _userService.SetUserInfoUnlockProcessDate(username);
                    _logger.LogError($"Account unlocked successfully.{username}");

                    //alttaki session mesajı success mesajı ekranda gostermeye yarar ve mesajda session ile taşıdım
                    EnableSucessMessageForUnLock();
                    _httpContextAccessor.HttpContext?.Session.SetString("Session_success_alert_message_Content_unlock", _languageService["unlockSucessfully"].ToString());
                    return Json(1);
                }
                _httpContextAccessor.HttpContext?.Session.Remove("unlock_username");
                _httpContextAccessor.HttpContext?.Session.Remove("unlcok_smscode");
                _httpContextAccessor.HttpContext?.Session.Remove("unlock_errorTryCount");
            }
            else
            {
                if (errorTryCount == 0)
                {
                    _httpContextAccessor.HttpContext?.Session.SetInt32("unlock_errorTryCount", 1);
                }
                else
                {
                    _httpContextAccessor.HttpContext?.Session.SetInt32("unlock_errorTryCount", errorTryCount + 1);
                }
                if (errorTryCount < 3)
                {
                    _logger.LogError($"Sms verification code is not correct");
                    errorMessage.Add(_languageService["smsVerificationError"].ToString());
                    return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
                }
                else
                {
                    _logger.LogError($"Sms verification code is not correct for 3 times, session cleared");
                    errorMessage.Add(_languageService["repeatedSmsCodeMessage"].ToString());
                    return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
                }
            }
        }
        errorMessage = resultvalidation.Errors.Select(x => x.ErrorMessage).ToList();
        return BadRequest(ResponseData<string>.Fail(StatusCodes.Status400BadRequest, errorMessage));
    }

    [NonAction]
    private void SetPassword(PrincipalContext context, IOptions<Settings> _options, string username, string newPassword)
    {
        UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
        user.Enabled = true;
        user.SetPassword(newPassword);
        user.ExpirePasswordNow();
        user.Save();
    }
    [NonAction]
    private void DisableSucessMessageForResetPassword()
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32("Session_success_alert_message", 0);
    }
    [NonAction]
    private void EnableSucessMessageForResetPassword()
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32("Session_success_alert_message", 1);
    }
    [NonAction]
    private void DisableSucessMessageForUnLock()
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32("Session_success_alert_message_unlock", 0);
    }
    [NonAction]
    private void EnableSucessMessageForUnLock()
    {
        _httpContextAccessor.HttpContext?.Session.SetInt32("Session_success_alert_message_unlock", 1);
    }

    [HttpGet]
    public string GenerateCaptcha()
    {
        string captcha = _userService.GenerateCaptcha(_options.Value.CaptchaLength);
        return captcha;
    }
}
