namespace WebUI.Abstract;
public interface IUserService
{
    int GetUserInfoCountPassWord(string username);
    int GetUserInfoCountUnlock(string username);
    bool DoesUserExist(string userName, string AdKey);
    PrincipalContext GetContext(string key);
    DateTime GetUserLoginTimeForPassWord(string username);
    DateTime GetUserLoginTimeForUnlock(string username);
    DateTime GetLastPwdChangeDate(string username);
    string GetMobilePhone(string username);
    string CreateSmsCode();
    string GetBrowserLanguage();
    string GetPhoneFormat(string phone);
    void SetUserInfoPassWordProcessDate(string username);
    void SetUserInfoUnlockProcessDate(string username);
    List<UserInfo> GetUserPasswordDailyHistory(string username, DateTime datetime);
    string GenerateCaptcha(int length);

}

