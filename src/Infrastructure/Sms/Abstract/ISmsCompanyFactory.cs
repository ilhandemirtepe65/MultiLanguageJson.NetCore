
namespace Sms.Abstract;
public interface ISmsCompanyFactory
{
    ISmsCompany ProduceSmsCompany(CompanyType companyType);
}

