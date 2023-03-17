using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        public AccountController(
            IMapper mapper,
            UserManager<User> userManager,
            IConfiguration configuration,
            IAuthService service
        )
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _service = service;
        }

        [HttpPost]
        //[ServiceFilter(typeof(UserRegistrationModel))] // xem lai
        public async Task<IActionResult> Register(UserRegistrationModel userModel)
        {
            try
            {
                var user = _mapper.Map<User>(userModel);
                await _service.RegisterAsync(user);
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
    }
}
