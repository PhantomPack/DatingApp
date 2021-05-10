using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq; 
namespace API.Services
{

    public class TokenService : ITokenService
    {

        private readonly SymmetricSecurityKey _key;

        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

        }


        public async Task<string> CreateToken(AppUser user)
        {

            // Add claims

            var claims = new List<Claim>
            {

                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)

            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            
            // Create credentials


            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);


            // Describe how the token will look

            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.Now.AddDays(7),

                SigningCredentials = creds

            };


            // Create token handler

            var tokenHandler = new JwtSecurityTokenHandler();


            // Create the token

            var token = tokenHandler.CreateToken(tokenDescriptor);


            // Return the written token

            return tokenHandler.WriteToken(token);


        }
    
}

}

