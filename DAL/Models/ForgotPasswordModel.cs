using System.ComponentModel.DataAnnotations;

namespace Task_2EF.DAL.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
