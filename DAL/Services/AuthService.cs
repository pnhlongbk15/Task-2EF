using Microsoft.AspNetCore.Identity;
using Task_2EF.DAL.Entities;
using Task_2EF.DAL.Repository;

namespace Task_2EF.DAL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        public AuthService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<Object> LoginAsync(User user)
        {
            try
            {
                var result = await _userManager.FindByEmailAsync(user.Email);
                if (result != null && await _userManager.CheckPasswordAsync(result, user.PasswordHash))
                {
                    var userRoles = await _userManager.GetRolesAsync(result);
                    Console.WriteLine("roles:");
                    foreach (var role in userRoles)
                    {
                        Console.WriteLine(role.ToString());
                    }
                    return new { user = result, roles = userRoles };
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            throw new Exception("Email or password is invalid.");
        }

        public async Task RegisterAsync(User user)
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to register {ex.Message}");
            }
        }
    }
}
