using Task_2EF.DAL.Entities;

namespace Task_2EF.DAL.Repository
{
    public interface IAuthService
    {
        async Task RegisterAsync(User user) { }
        Task<Object> LoginAsync(User user);
    }
}
