using BussinessLayer.Interface;
using CommonLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Service
{
    public class UserBlImpl:IUserBl
    {
        private readonly string jwtSecretKey;
        private readonly IUserRl userRl;

        public UserBlImpl(IUserRl userRl, IConfiguration configuration)
        {
            this.userRl = userRl;
            jwtSecretKey = configuration["JwtSettings:SecretKey"];
        }

        //User Login
        public string userLogin(UserLogin userlogin)
        {
            var user=userRl.userLogin(userlogin);
            if (user)
                return "Login Successfull";
            else
                return "Login Unsuccessfull";
        }
        
        //Registraion of new User
        public string userRegistration(User user)
        {
            int changes = userRl.userRegistration(user);
            if (changes > 0)
                return "Registration Successfull";
            else
                return "Registration Unsuccessfull";
        }
        public string existsUser(User user)
        {
            var userExists = userRl.userExist(user);
            return userExists;
            
        } 

        public string getByName(string name)
        {
            var userExists=userRl.getByName(name);
            return userExists;
        }
        //Genrating token for username
        public string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,userName)
                   
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
