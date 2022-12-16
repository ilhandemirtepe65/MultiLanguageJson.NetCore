namespace WebUI.Validators;
public class SmsValidation : AbstractValidator<SmsViewModel>
{
    private readonly IStringLocalizer<UserValidation> _languageService;
    public SmsValidation(IStringLocalizer<UserValidation> languageService)
    {
        _languageService = languageService;
        RuleFor(c => c.SmsCode).NotEmpty().WithMessage(_languageService["smsValidationMessage"].ToString());
    }
}

