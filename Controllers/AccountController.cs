using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Models;

namespace Task_2EF.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(IMapper mapper, UserManager<User> userManager, IConfiguration configuration)
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        //[ServiceFilter(typeof(UserRegistrationModel))] // xem lai
        public async Task<IActionResult> Register(UserRegistrationModel userModel)
        {
            var IsExist = await _userManager.FindByEmailAsync(userModel.Email);
            if (IsExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            new { Status = "Error", Message = "User already exists!" });
            }

            var user = _mapper.Map<User>(userModel);
            var result = await _userManager.CreateAsync(user, userModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            await _userManager.AddToRoleAsync(user, "Visitor");
            return Ok("Register successfull.");

        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel userModel)
        {
            var user = await _userManager.FindByEmailAsync(userModel.Email);
            if (user != null &&
                await _userManager.CheckPasswordAsync(user, userModel.Password))
            {
                var authClaims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };
                

                var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(20),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authenKey,
                                            SecurityAlgorithms.HmacSha512Signature)
                    );


                return Ok(new
                {
                    api_key = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    status = "Login successfully."
                });
            }
            else
            {
                return BadRequest("Invalid UserName or Password");
            }
        }
    }
}
