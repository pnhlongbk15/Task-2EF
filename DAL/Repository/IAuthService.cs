using Task_2EF.DAL.Entities;

namespace Task_2EF.DAL.Repository
{
    public interface IAuthService
    {
        Task<String> RegisterAsync(User user);
        Task<String> LoginAsync(User user);
        Task<Object> LoginStepTwo(string twoFactorCode, string email);
    }
}
