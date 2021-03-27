
using System;

using System.Collections.Generic;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

using API.Entities;

using API.Interfaces;

using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;


namespace API.Services
{

    public class TokenService : ITokenService
    {

        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

        }


        public string CreateToken(AppUser user)
        {

            // Add claims

            var claims = new List<Claim>
            {

                new Claim(JwtRegisteredClaimNames.NameId, user.UserName)

            };


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

