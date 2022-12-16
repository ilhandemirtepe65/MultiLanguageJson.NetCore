namespace Sms.Concrete;
public class SmsCompanyFactory : ISmsCompanyFactory
{
    public ISmsCompany ProduceSmsCompany(CompanyType companyType)
    {
        ISmsCompany smsCompany = null;
        switch (companyType)
        {
            
            case CompanyType.Custom:
                smsCompany = new CustomProvider();
                break;
        }
        return smsCompany;
    }

    
}

