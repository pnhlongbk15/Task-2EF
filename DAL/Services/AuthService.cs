using Microsoft.AspNetCore.Identity;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_2EF.DAL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<String> LoginAsync(User user)
        {
            try
            {
                var aResult = await _userManager.FindByEmailAsync(user.Email);
                if (aResult == null)
                {
                    throw new Exception("Invalid Email.");
                }
                Console.WriteLine(user.Email);
                Console.WriteLine(user.PasswordHash);

                await _signInManager.SignOutAsync();
                var signInResult = await _signInManager.PasswordSignInAsync(aResult, user.PasswordHash, false, true);
                //var signInResult = await _signInManager.CheckPasswordSignInAsync(aResult, user.PasswordHash, true);
                //var check = await _userManager.CheckPasswordAsync(result, user.PasswordHash);

                if (!signInResult.Succeeded)
                {
                    if (signInResult.IsLockedOut == true)
                    {
                        throw new Exception("Once sucrity, your account is lockout. Pleases try again after 2 minuses.");
                    }
                    throw new Exception("Invalid password.");
                }
                var TwoFactorEnable = await _userManager.GetTwoFactorEnabledAsync(aResult);

                var providers = await _userManager.GetValidTwoFactorProvidersAsync(aResult);
                if (!providers.Contains("Email"))
                {
                    throw new Exception("Please try again.");
                }

                var token = await _userManager.GenerateTwoFactorTokenAsync(aResult, "Email");
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Object> LoginStepTwo(string token, string email)
        {
            var userAuthen = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (userAuthen == null)
            {
                throw new Exception("No user authen");
            }
            var result = await _signInManager.TwoFactorSignInAsync("Email", token, false, false);
            //_signInManager.TwoFactorSignInAsync()
            if (!result.Succeeded)
            {
                if (result.IsLockedOut == true)
                {
                    throw new Exception("Your account is lock");
                }
                throw new Exception("Please verify again.");
            }
            var user = await _userManager.FindByEmailAsync(email);
            var userRoles = await _userManager.GetRolesAsync(user);
            return new { user = user, roles = userRoles };
        }
        public async Task<String> RegisterAsync(User user)
        {
            try
            {
                var IsExist = await _userManager.FindByEmailAsync(user.Email);
                if (IsExist != null)
                {
                    throw new Exception("User already exists!");
                }

                var result = await _userManager.CreateAsync(user, user.PasswordHash);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        throw new Exception(error.Description);
                    }
                }
                await _userManager.AddToRoleAsync(user, "Visitor");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register {ex.Message}");
            }
        }
    }
}
