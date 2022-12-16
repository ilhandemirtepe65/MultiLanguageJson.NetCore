namespace WebUI;
public class Settings
{
    public string VTryCount { get; set; } = String.Empty;
    public string VTryTime { get; set; } = String.Empty;
    public string VDomainContainer { get; set; } = String.Empty;
    public string VLDAPUser { get; set; } = String.Empty;
    public string VLDAPPass { get; set; } = String.Empty;
    public string VDomainUser { get; set; } = String.Empty;
    public string VDomainPass { get; set; } = String.Empty;
    public string VSMSPasswordLenght { get; set; } = String.Empty;
    public string VPasswordCharset { get; set; } = String.Empty;
    public string VLDAPPath { get; set; } = String.Empty;
    public string VVerificationCodeTimeout { get; set; } = String.Empty;
    public string DatabaseLocation { get; set; } = String.Empty;
    public string AesKey { get; set; } = String.Empty;
    public string ServiceAccount { get; set; } = String.Empty;
    public string PaswordLength { get; set; } = String.Empty;
    public string ADIpNumber { get; set; } = String.Empty;
    public string ADKey { get; set; } = String.Empty;//active directory işlemlerinde enc ve decription için kullanılır
    public string Logo { get; set; } = String.Empty;
    public string Title { get; set; } = String.Empty;
    public string DailyPasswordChanceCount { get; set; } = String.Empty;
    public int CaptchaLength { get; set; } = 0;
}











