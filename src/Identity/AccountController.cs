using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using LClaproth.MyFinancialTracker.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LClaproth.MyFinancialTracker.Identity
{

    public class AccountCreationParams
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }

    public class AuthenticationParams
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RegistrationService<IdentityUser> _registrationService;
        private readonly AuthenticationService<IdentityUser> _authenticationService;
        public AccountController(RegistrationService<IdentityUser> registrationService, AuthenticationService<IdentityUser> authenticationService)
        {
            _registrationService = registrationService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetSignedInUser()
        {
            // var user = context.User;
            var user = HttpContext.User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            return Ok(new Response<string>(user, null){Message = "Ok."});
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountCreationParams user)
        {
            try
            {
                var result = await _registrationService.Register(user);
                return Ok(new Response<string>(result, null) { Message = "Ok. Successfully created user." });
            }
            catch (Exception e)
            {
                Response<Exception> response = new Response<Exception>(null, e)
                {
                    Message = "An error occurred while creating a user."
                };

                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationParams parameters){
            try{
                var result = await _authenticationService.Authenticate(parameters);
                return Ok(new Response<Dictionary<string, string>>(new Dictionary<string, string> {{"token",result}}, null){Message = "Successfully logged in."});
                
            } catch (Exception e){
                return BadRequest(new Response<Exception>(null, e) { Message = "An error occurred" });
            }
        }
    }
}