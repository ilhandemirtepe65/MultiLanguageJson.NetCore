namespace WebUI.Validators;
public class UserValidation : AbstractValidator<UserViewModel>
{
    private readonly IOptions<Settings> _options;
    private readonly IStringLocalizer<UserValidation> _languageService;
    public UserValidation(IOptions<Settings> options, IStringLocalizer<UserValidation> languageService)
    {
        _options = options;
        _languageService = languageService;
        int passwordLength = Convert.ToInt32(_options.Value.PaswordLength);
        RuleFor(c => c.UserName).NotEmpty().WithMessage(_languageService["usernameValidationMessage"].ToString()); 
        RuleFor(c => c.Captcha).NotEmpty().WithMessage(_languageService["captchaValidationMessage"].ToString()); 
        RuleFor(c => c.PasswordConfirm).NotEmpty().WithMessage(_languageService["passwordConfirmValidationMessage"].ToString()); ;
        RuleFor(c => c.Password).NotEmpty().WithMessage(_languageService["passwordValidationMessage"].ToString()).MinimumLength(passwordLength).WithMessage($"{_languageService["passwordLengthMessage"].ToString()}  {passwordLength}   {languageService["mustBe"].ToString()}");
        RuleFor(c => c.CurrentPassword).NotEmpty().WithMessage(_languageService["currentPasswordValidationMessage"].ToString());
        RuleFor(x => x.PasswordConfirm).Equal(x => x.Password).WithMessage(_languageService["passwordConfirm"].ToString());
    }
}
