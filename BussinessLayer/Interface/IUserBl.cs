using CommonLayer.Models;
using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface IUserBl
    {
        string userRegistration(User user);
        string userLogin(UserLogin user);
        string GenerateJwtToken(string userName);
        string existsUser(User user);

       string getByName(string name);

    }
}
