using CommonLayer.Models;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Service
{
    public class UserRlImpl : IUserRl
    {
        private readonly static string key = "adef@wwaerds";
        private readonly UserContext userContex;
        public UserRlImpl(UserContext context)
        {
            userContex = context;
        }

        public int userRegistration(User user)
        {
            string passwordEncrypt = convertToEncrypt(user.password);
            var userEntity = new UserEntity()
            {
                name = user.name,
                email = user.email,
                role = user.role,
                password = passwordEncrypt

            };
            userContex.Users.Add(userEntity);
           int changes=userContex.SaveChanges();
            return changes; 
        }
        public bool userLogin(UserLogin userlogin)
        {

            string passwordDecrypt= convertToDecrypt(userlogin.password); 
            var user=userContex.Users.SingleOrDefault(x => x.email.Equals(userlogin.username) && x.password.Equals(passwordDecrypt));
            return user != null;
        }
        
        public string userUpdate(User user)
        {
            string passwordEncrypt = convertToEncrypt(user.password);
            var userEntity = new UserEntity()
            {
                name = user.name,
                email = user.email,
                role = user.role,
                password = passwordEncrypt

            };
            userContex.Users.Update(userEntity);
            userContex.SaveChanges();
            return "User Updated";
        }
        public string userExist(User user)
        {
            var userExists = userContex.Users.SingleOrDefault(x => x.email.Equals(user.email));
            if(userExists != null)
            {
                userUpdate(user);
                return "User Updated Sucessfully";
            }
            else
            {
                userRegistration(user);
                return "User Added Sucessfully";
            }
            
        }

        public string getByName(string name)
        {
            var userByName = userContex.Users.SingleOrDefault(x => x.name.Equals(name));
            if (userByName != null)
                return userByName.id+"\n"+userByName.name+"\n"+userByName.email;
            else
                return "UserNotPresent";
        }
        //Encrypting password
        public string convertToEncrypt(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";
            password += key;
            var passwordEncrypt=Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(passwordEncrypt); 
        }
        //Decrypting password
       public  string convertToDecrypt(string password)
       {
            if (string.IsNullOrEmpty(password))
            {
                return null; 
            }

            try
            {
                var passwordBytes = Convert.FromBase64String(password);
                var result = Encoding.UTF8.GetString(passwordBytes);

               
                if (!string.IsNullOrEmpty(key) && result.Length >= key.Length)
                {
                    result = result.Substring(0, result.Length - key.Length);
                }

                return result;
            }
            catch (FormatException ex)
            {
                
                Console.WriteLine("Error decoding Base64 string: " + ex.Message);
                return null;
            }
        }
    }
}
