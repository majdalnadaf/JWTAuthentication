using JWTAuthentication.DataTransferObjects;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTAuthentication.Services
{
    public class AuthService : IAuthService
    {
        #region Ctor
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _roleManager = roleManager;
        } 
        #endregion


        #region Create Token
        public async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser model)
        {
            var userClaims = await _userManager.GetClaimsAsync(model);
            var roles = await _userManager.GetRolesAsync(model);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("role", role));


            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub , model.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, model.Email!),
                new Claim("uid", model.Id)
            }.Union(userClaims)
             .Union(roleClaims);

            var symmmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var signInCredential = new SigningCredentials(symmmetricSecurityKey, SecurityAlgorithms.HmacSha256);


            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays),
                signingCredentials: signInCredential

                );

            return jwtSecurityToken;

        } 
        #endregion

        #region Login
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var oAuthModel = new AuthModel();
                oAuthModel.Errors.Add("Email or Password is incorrect");
                return oAuthModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            return new AuthModel
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiereOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = userRoles.ToList()
            };


        } 
        #endregion

        #region Register

        public async Task<AuthModel> RegisterAsync(RegisterUserModel model)
        {
            try
            {
                if (await _userManager.FindByEmailAsync(model.Email) is not null)
                    return new AuthModel { Errors = new List<string>() { "The user is already registered!" } };

                if (await _userManager.FindByNameAsync(model.UserName) is not null)
                    return new AuthModel { Errors = new List<string>() { "The username is already registered!" } };

                var user = new ApplicationUser
                {

                    FirstName = model.UserName,
                    LastName = model.UserName,
                    Email = model.Email,
                    UserName = model.UserName,
                };


                var regiesterResult = await _userManager.CreateAsync(user, model.Password);
                if (!regiesterResult.Succeeded)
                {
                    var oAuthModel = new AuthModel();

                    foreach (var error in regiesterResult.Errors)
                    {
                        oAuthModel.Errors.Add(error.Description);
                    }


                    return oAuthModel;
                }




                if (!await _roleManager.RoleExistsAsync("User"))
                {
                  
                    await _roleManager.CreateAsync(new IdentityRole() { Name = "User" });
                  
                }

                await _userManager.AddToRoleAsync(user, "User");
                var jwtSecurityToken = await CreateJwtToken(user);

                return new AuthModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    ExpiereOn = jwtSecurityToken.ValidTo,
                    IsAuthenticated = true,
                    Email = user.Email,
                    UserName = user.UserName

                };
            }
            catch (Exception)
            {

                throw new Exception();
            }
           

        } 
        #endregion




    }
}
