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
                //var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
                //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                //identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                //await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                //new ClaimsPrincipal(identity));
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            //new Claim(ClaimTypes.MobilePhone,user.PhoneNumber),
                            new Claim(ClaimTypes.Email,user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JsonWebTokenKeys:IssuerSigninKey"]));
                var token = new JwtSecurityToken(
                             expires: DateTime.Now.AddHours(3),
                             claims: authClaims,
                             signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                         );


                return Ok(new
                {
                    api_key = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    Role = userRoles,
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
