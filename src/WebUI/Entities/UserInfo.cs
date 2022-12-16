namespace WebUI.Entities;
public class UserInfo
{
    [BsonId(true)]
    public int Id { get; set; }
    public string Username { get; set; } = String.Empty;
    public DateTime PassWordProcessDate { get; set; }
    public DateTime UnlockProcessDate { get; set; }
}

