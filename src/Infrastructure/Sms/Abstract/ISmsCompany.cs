
namespace Sms.Abstract;
public interface ISmsCompany
{
    bool SendSms(string message, string mobilePhone);
}

