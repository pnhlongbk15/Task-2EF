using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Models;
using Task_2EF.DAL.Repository;

namespace Task_2EF.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _service;
        private readonly IEmailSender _emailSender;

        public AccountController(
            IMapper mapper,
            UserManager<User> userManager,
            IConfiguration configuration,
            IAuthService service,
            IEmailSender emailSender
        )
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _service = service;
            _emailSender = emailSender;
        }

        [HttpPost]
        //[ServiceFilter(typeof(UserRegistrationModel))] // xem lai
        public async Task<IActionResult> Register(UserRegistrationModel userModel)
        {
            try
            {
                var user = _mapper.Map<User>(userModel);
                var token = await _service.RegisterAsync(user);
                var confirmUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { token, email = user.Email },
                    Request.Scheme
                );

                var contentEmail = "Please click here: <a href=\"#URL#\">Click here.</a>";
                contentEmail = contentEmail.Replace("#URL#", confirmUrl);

                // send
                await _emailSender.SendEmailAsync(
                    userModel.Email,
                    "Authen your account.",
                    contentEmail
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Register successfully.");
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel userModel)
        {
            try
            {
                var user = _mapper.Map<User>(userModel);
                dynamic result = await _service.LoginAsync(user);

                var roles = result.roles;
                user = result.user;

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                foreach (var r in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, r));
                }

                var authSigninKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JsonWebTokenKeys:IssuerSigninKey"])
                );
                var token = new JwtSecurityToken(
                    issuer: _configuration["JsonWebTokenKeys:ValidIssuer"],
                    audience: _configuration["JsonWebTokenKeys:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: new SigningCredentials(
                        authSigninKey,
                        SecurityAlgorithms.HmacSha256
                    ),
                    claims: authClaims
                );

                return Ok(
                    new
                    {
                        api_key = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        Role = roles,
                        status = "Login successfully."
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest("Invalid email confirmation url.");
            }
            var aUser = await _userManager.FindByEmailAsync(email);
            if (aUser == null)
            {
                return BadRequest("Please try again later.");
            }

            var result = await _userManager.ConfirmEmailAsync(aUser, token);
            var status = result.Succeeded
                ? "Thank you for confirming your mail"
                : "Your email is not confirmed, please try again later.";

            return Ok(status);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel mForgotPassword)
        {
            if (mForgotPassword == null)
            {
                return BadRequest("Please entry your email.");
            }
            try { 
                var aUser = await _userManager.FindByEmailAsync(mForgotPassword.Email);
                if(aUser == null){
                    return BadRequest("Incorrect.")
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(aUser);

                var confirmUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { token, email = mForgotPassword.Email },
                    Request.Scheme
                );

                var contentEmail = "Please click here to retrive password: <a href=\"#URL#\">Click here.</a>";
                contentEmail = contentEmail.Replace("#URL#", confirmUrl);

                // send
                await _emailSender.SendEmailAsync(
                    mForgotPassword.Email,
                    "Retrive your account.",
                    contentEmail
                );
            }
            catch (Exception ex) { 
                return StatusCode(500, ex.Message);
            }

            return Ok("Check mail please.");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email){
            return Ok(new {
                token = token,
                email = email
            });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel mResetPassword){
            if (mResetPassword == null) {
                return BadRequest("you try again.");
            }
            
            try {

                var aUser = await _userManager.FindByEmailAsync(mResetPassword.Email);
                if(aUser == null) {
                    return BadRequest("User doesn't exist.");
                }
                var resetPassResult = await _userManager.ResetPasswordAsync(aUser, mResetPassword.Token, mResetPassword.Password);
                if(!resetPassResult.Succeeded) {
                    foreach (var error in resetPassResult.Errors)
                    {
                        ModelState.TryAddModelError(error.Code, error.Description);
                    }
                    return StatusCode(500, ModelState);
                }
                return Ok("Reset password successfully.");

            } catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
            
        }
    }   
}
