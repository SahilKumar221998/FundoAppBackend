using BusinessLayer.Interface;
using CommonLayers.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fundo_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;   
        private readonly IUserBl userBl;
        public UserController(IUserBl userBl,ILogger<UserController> logger)
        {
            this.userBl = userBl;
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult userRegistration(User user)
        {
            try
            {
                var result = userBl.userRegistration(user);

                if (result != null)
                {
                    //ok is HTTP respnse it has some format and is called 
                    return Ok(new { success = true, message = "Registration is Successful", data = result });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Registration is UnSuccessful" });

                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Route("UserLogin")]
        public IActionResult userLogin(UserLogin userLogin)
        {
            try
            {
                var resultLog = userBl.userLogin(userLogin);

                if (resultLog != null)
                {
                    return Ok(new { success = true, message = "Login Successful", data = resultLog });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Login UnSuccessful" });
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("forgetPassword")]
        public Task<string> forgetPassword(string email)
        {
            return userBl.forgetPassword(email);    
        }

        
        [HttpPut]
        [Route("resetPassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult resetPassword(string newPassword,string confirmPassword)
        {
            try
            {
                var email= User.Claims.FirstOrDefault(x => x.Type.Equals("Email")).Value;
                Console.WriteLine(email);
                logger.LogInformation(email, confirmPassword);

                if (string.IsNullOrEmpty(email))
                {
                    logger.LogInformation("null email", email);
                    return BadRequest(new { success = false, message = "User email not found." });
                }

                // Perform password reset
                var resultLog = userBl.resetPassword(email, newPassword, confirmPassword);

                if (resultLog != null)
                {
                    return Ok(new { success = true, message = resultLog });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Password reset unsuccessful." });
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error resetting password: {0}", ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred while resetting password." });
            }
        }
    }
}
