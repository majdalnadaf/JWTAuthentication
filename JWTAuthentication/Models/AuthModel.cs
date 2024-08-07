namespace JWTAuthentication.Models
{
    public class AuthModel
    {
        public string Email { get; set; }

        public string UserName { get; set; }
        public List<string> Errors { get; set; }    
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
        public DateTime ExpiereOn { get; set; }
    }
}
