namespace WebUI.Concrete;
public class UserService : IUserService
{
    private readonly IOptions<Settings> _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(IOptions<Settings> options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }
    public int GetUserInfoCountPassWord(string username)
    {
        int userCount = 0;
        DateTime processDate = DateTime.Now.AddMinutes(-Convert.ToDouble(_options.Value.VTryTime.ToString()));
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var uI = db.GetCollection<UserInfo>("UserInfo");
            userCount = uI.FindAll().ToList().Where(x => x.Username == username && x.PassWordProcessDate > processDate).Count();
        }
        return userCount;
    }
    public int GetUserInfoCountUnlock(string username)
    {
        int userCount = 0;
        DateTime processDate = DateTime.Now.AddMinutes(-Convert.ToDouble(_options.Value.VTryTime.ToString()));
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var uI = db.GetCollection<UserInfo>("UserInfo");
            userCount = uI.FindAll().ToList().Where(x => x.Username == username && x.UnlockProcessDate > processDate).Count();
        }
        return userCount;
    }
    public bool DoesUserExist(string userName, string Adkey)
    {
        using (var domainContext = GetContext(Adkey))
        {
            using (var userPrincipal = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, userName))
            {
                if (userPrincipal != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    public PrincipalContext GetContext(string Adkey)
    {
        PrincipalContext context = new PrincipalContext(ContextType.Domain,
            _options.Value.ADIpNumber,//name
            _options.Value.VDomainContainer,//container
            _options.Value.VDomainUser, //username
            PasswordReset.DecryptString(Adkey, _options.Value.VDomainPass));
        return context;

        //PrincipalContext context = new PrincipalContext(ContextType.Domain, "192.168.245.128:389", "dc=test,dc=local", "administrator", "VAN65van");
        //return context;
    }
    public DateTime GetUserLoginTimeForPassWord(string username)
    {
        DateTime loginDate = DateTime.Now;
        DateTime processDate = DateTime.Now.AddMinutes(-Convert.ToInt32(_options.Value.VTryTime));
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var uI = db.GetCollection<UserInfo>("UserInfo");
            loginDate = uI.FindAll().ToList().Where(x => x.Username == username && x.PassWordProcessDate > processDate).OrderByDescending(x => x.PassWordProcessDate).Take(1).FirstOrDefault().PassWordProcessDate;
        }
        return loginDate;
    }
    public DateTime GetUserLoginTimeForUnlock(string username)
    {
        DateTime loginDate = DateTime.Now;
        DateTime processDate = DateTime.Now.AddMinutes(-Convert.ToInt32(_options.Value.VTryTime));
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var uI = db.GetCollection<UserInfo>("UserInfo");
            loginDate = uI.FindAll().ToList().Where(x => x.Username == username && x.UnlockProcessDate > processDate).OrderByDescending(x => x.PassWordProcessDate).Take(1).FirstOrDefault().PassWordProcessDate;
        }
        return loginDate;
    }
    public DateTime GetLastPwdChangeDate(string username)
    {
        DateTime pwdLastSet = DateTime.Now;
        using (DirectoryEntry dirEntry = new DirectoryEntry(_options.Value.VLDAPPath, _options.Value.VLDAPUser, PasswordReset.DecryptString(_options.Value.ADKey, _options.Value.VLDAPPass), AuthenticationTypes.Secure))
        {
            dirEntry.RefreshCache();
            using (DirectorySearcher searcher = new DirectorySearcher(dirEntry))
            {
                searcher.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", username);
                SearchResult result = searcher.FindOne();
                if (result != null)
                {
                    long value = (long)result.Properties["pwdLastSet"][0];
                    pwdLastSet = DateTime.FromFileTimeUtc(value);
                }
            }
        }
        return pwdLastSet;
    }
    public string GetMobilePhone(string username)
    {
        string telephoneNumber = String.Empty;
        using (DirectoryEntry dirEntry = new DirectoryEntry(_options.Value.VLDAPPath, _options.Value.VLDAPUser, PasswordReset.DecryptString(_options.Value.ADKey, _options.Value.VLDAPPass), AuthenticationTypes.Secure))
        {
            dirEntry.RefreshCache();
            using (DirectorySearcher searcher = new DirectorySearcher(dirEntry))
            {
                searcher.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", username);
                SearchResult result = searcher.FindOne();
                if (result != null)
                {
                    using (DirectoryEntry userEntry = result.GetDirectoryEntry())
                    {
                        telephoneNumber = userEntry.Properties["telephoneNumber"][0].ToString();
                    }
                }
            }
        }
        return telephoneNumber;
    }
    public string CreateSmsCode()
    {
        string code = "";
        string charset = _options.Value.VPasswordCharset;
        for (int i = 0; i < int.Parse(_options.Value.VSMSPasswordLenght); i++)
        {
            code += charset[GenerateRandomInt(0, charset.Length - 1)];
        }
        return code;
    }
    public int GenerateRandomInt(int minVal = 0, int maxVal = 100)
    {
        var rnd = new byte[4];
        using (var rng = new RNGCryptoServiceProvider())
            rng.GetBytes(rnd);
        var i = Math.Abs(BitConverter.ToInt32(rnd, 0));
        return Convert.ToInt32(i % (maxVal - minVal + 1) + minVal);
    }

    public string GetBrowserLanguage()
    {
        string cookieValueFromContext = _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Culture"];
        if (!string.IsNullOrEmpty(cookieValueFromContext))
        {
            return cookieValueFromContext.Substring(2, 5);
        }
        else
        {
            string culture = string.Empty;
            string languages = _httpContextAccessor?.HttpContext?.Request.Headers["Accept-Language"].FirstOrDefault();//browser dili
            if (!string.IsNullOrEmpty(languages) && languages.Contains(","))//browser multiple language
            {
                string splitLanguage = languages.Split(",")[0];
                if (splitLanguage == "en")
                {
                    culture = splitLanguage + "-" + "US";
                }
                else
                {
                    culture = splitLanguage + "-" + splitLanguage.ToUpper();
                }
            }
            else
            {
                if (languages == "en")
                {
                    culture = languages + "-" + "US";
                }
                else
                {
                    culture = languages + "-" + languages.ToUpper();
                }
            }
            return culture;
        }
    }
    public string GetPhoneFormat(string phone)
    {
        string formatPhone = "";
        if (phone.StartsWith("0"))
        {
            phone.Remove(0, 1);//remove 0
        }
        if (phone.StartsWith("+90"))
        {
            formatPhone = phone;
        }
        else
        {
            formatPhone = "+90" + phone;
        }
        formatPhone = String.Concat(formatPhone.Where(c => !Char.IsWhiteSpace(c)));// RemoveWhitespace(formatPhone);
        string data1 = formatPhone.Substring(0, 3);// +90
        string data2 = formatPhone.Substring(3, 3);//539
        string data3 = formatPhone.Substring(6, 3).Replace(formatPhone.Substring(6, 3), "***");//299-->***
        string data4 = formatPhone.Substring(9, 2).Replace(formatPhone.Substring(9, 2), "**");//03-->**
        string data5 = formatPhone.Substring(11, 2);//97
        string formatPhoneResult = string.Format("{0} {1} {2} {3} {4}", data1, data2, data3, data4, data5);
        return formatPhoneResult;
    }


    public void SetUserInfoPassWordProcessDate(string username)
    {
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var userInfoCollection = db.GetCollection<UserInfo>("UserInfo");
            UserInfo userinfo = new UserInfo()
            {
                Username = username,
                PassWordProcessDate = DateTime.Now
            };
            userInfoCollection.Insert(userinfo);
        }
    }
    public void SetUserInfoUnlockProcessDate(string username)
    {
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var userInfoCollection = db.GetCollection<UserInfo>("UserInfo");
            UserInfo userinfo = new UserInfo()
            {
                Username = username,
                UnlockProcessDate = DateTime.Now
            };
            userInfoCollection.Insert(userinfo);
        }
    }
    public List<UserInfo> GetUserPasswordDailyHistory(string username, DateTime datetime)
    {
        List<UserInfo> userInfos = new List<UserInfo>();
        using (var db = new LiteDatabase(_options.Value.DatabaseLocation))
        {
            var userinfo = db.GetCollection<UserInfo>("UserInfo");
            userInfos = userinfo.FindAll().Where(x => x.Username == username && x.PassWordProcessDate >= datetime).ToList();
        }
        return userInfos;
    }

    public string GenerateCaptcha(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
