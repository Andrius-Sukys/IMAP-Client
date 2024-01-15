namespace KT_IMAPClient;
public class UserInfo
{
    public UserInfo(string email, string password)
    {
        Email = email;
        Password = password;
    }
    public string Password { get; set; }
    public string Email { get; set; }
}
