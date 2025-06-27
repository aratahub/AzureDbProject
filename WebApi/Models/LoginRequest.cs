namespace WebApi.Models
{
    public class LoginRequest
    {
        public string ClientUserName { get; set; } = string.Empty;
        public string ClientPassword { get; set; } = string.Empty;
    }
}