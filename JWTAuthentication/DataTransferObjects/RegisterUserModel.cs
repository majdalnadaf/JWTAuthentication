using System.ComponentModel.DataAnnotations;

namespace JWTAuthentication.DataTransferObjects
{
    public class RegisterUserModel
    {
        [Required , MaxLength(200)]
        public string FirstName { get; set; }
        
        [Required, MaxLength(200)]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required , EmailAddress , MaxLength(200)]
        public string Email { get; set; }
        [Required, MaxLength(200)]
        public string Password { get; set; }


    }
}
