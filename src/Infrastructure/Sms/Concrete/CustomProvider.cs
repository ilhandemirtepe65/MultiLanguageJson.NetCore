
using Newtonsoft.Json;
namespace Sms.Concrete;
public class CustomProvider : ISmsCompany
{
    public bool SendSms(string message, string mobilePhone)
    {
        var sendMsg = new SmsIstegi();
        sendMsg.username = "902129630957";
        sendMsg.password = "DataMarket1994_";
        sendMsg.source_addr = "DATAMARKET";
        //sendMsg.messages = new Mesaj[] { new Mesaj(Message, "905076064933") };
        sendMsg.messages = new Mesaj[] { new Mesaj(message, mobilePhone) };
        string payload = JsonConvert.SerializeObject(sendMsg);
        WebClient wc = new WebClient();
        wc.Headers["Content-Type"] = "application/json";
        string campaign_id = wc.UploadString("http://sms.verimor.com.tr/v2/send.json", payload);

        return true;
    }
}

class Mesaj
{
    public string msg { get; set; }
    public string dest { get; set; }
    public Mesaj() { }
    public Mesaj(string msg, string dest)
    {
        this.msg = msg;
        this.dest = dest;
    }
}
class SmsIstegi
{
    public string username { get; set; }
    public string password { get; set; }
    public string source_addr { get; set; }
    public Mesaj[] messages { get; set; }
}


