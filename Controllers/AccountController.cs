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
                var confirmUrl = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);

                var contentEmail = "Please click here: <a href=\"#URL#\">Click here.</a>";
                contentEmail = contentEmail.Replace("#URL#", confirmUrl);

                // send
                await _emailSender.SendEmailAsync(userModel.Email, "Authen your account.", contentEmail);
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
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Please try again later.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            var status = result.Succeeded ? "Thank you for confirming your mail" :
                                            "Your email is not confirmed, please try again later.";

            return Ok(status);
        }
    }
}
