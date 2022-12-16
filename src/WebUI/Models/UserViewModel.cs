namespace WebUI.Models;
public class UserViewModel
{
    public string UserName { get; set; } = ""; 
    public string Password { get; set; } = "";
    public string CurrentPassword { get; set; } = "";
    public string PasswordConfirm { get; set; } = "";
    public string Captcha { get; set; } = "";
}

