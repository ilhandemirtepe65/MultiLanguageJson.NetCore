using WebUI.Controllers;
namespace WebUI.Validators;
public class UnlockValidation : AbstractValidator<UnlockViewModel>
{
    private readonly IStringLocalizer<UnlockValidation> _languageService;
    public UnlockValidation(IStringLocalizer<UnlockValidation> languageService)
    {
        _languageService = languageService;
        RuleFor(c => c.UserName).NotEmpty().WithMessage(_languageService["usernameValidationMessage"].ToString()); 
        RuleFor(c => c.Captcha).NotEmpty().WithMessage(_languageService["captchaValidationMessage"].ToString());
    }
}

