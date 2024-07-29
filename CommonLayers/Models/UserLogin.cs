using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLayers.Models
{
    //<Summary>
    //Created a model calss for user login with email and password
    //<Summary>
    public class UserLogin
    {
        //Properties for user login
        [Required]
        public string Email { get; set;}
        
        [Required]
        public string Password { get; set;} 
    }
}
