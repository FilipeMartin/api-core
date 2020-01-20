using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using taesa_aprovador_api.Models;

namespace taesa_aprovador_api.Core
{
    public class TokenJWT : ITokenJWT
    {
        private readonly IConfiguration Configuration;

        public TokenJWT(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public object create(User user, string type)
        {
            var claims = new List<Claim>();

            if(user != null){
                claims.Add(new Claim(ClaimTypes.Name, user.Email));
                claims.Add(new Claim("UserID", user.Id.ToString()));
            } 

            claims.Add(new Claim("Type", type)); 

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration.GetSection("JWT").GetSection("SecurityKey").Value)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Configuration.GetSection("JWT").GetSection("Issuer").Value,
                audience: Configuration.GetSection("JWT").GetSection("Audience").Value,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(int.Parse(Configuration.GetSection("JWT").GetSection("DaysExpire").Value)),
                signingCredentials: creds
            );

            var message = new {
                token_type = "Bearer",
                created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                expiration = token.ValidTo.ToString("yyyy-MM-dd HH:mm:ss"),
                access_token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return message;
        }
    }
}