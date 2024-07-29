using CommonLayer.Models;
using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interface
{
    public interface IUserRl
    {
       int userRegistration(User user);
       bool userLogin(UserLogin user);
       string convertToEncrypt(string password);
       string convertToDecrypt(string password);
        string userUpdate(User user);
       string userExist(User user);
       string getByName(string name);
    }
}
