using JWTAuthentication.DataTransferObjects;
using JWTAuthentication.Models;
using System.IdentityModel.Tokens.Jwt;

namespace JWTAuthentication.Services
{
    public interface IAuthService
    {
        Task<JwtSecurityToken> CreateJwtToken(ApplicationUser model);
        Task<AuthModel> RegisterAsync(RegisterUserModel model);

        Task<AuthModel> GetTokenAsync(TokenRequestModel model);


    }
}
