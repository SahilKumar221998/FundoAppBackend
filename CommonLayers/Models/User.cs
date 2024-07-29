using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLayers.Models
{
    //<Sumarry>
    //Created a Model class For User
    //<Summary>
    public class User
    {

        //Properties of Users
        public long UserId { get; set; }
        
        [Required(ErrorMessage = "FirstName is required")]
        [StringLength(20, ErrorMessage = "FirstName must be at most 20 characters long")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        [StringLength(20, ErrorMessage = "LastName must be at most 20 characters long")]
        public string LastName { get; set; }
       
        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email must be at most 100 characters long")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role must be at most 50 characters long")]
        public string Password { get; set; }    
    }
}
