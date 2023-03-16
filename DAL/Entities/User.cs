using Microsoft.AspNetCore.Identity;

namespace Task_2EF.DAL.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
