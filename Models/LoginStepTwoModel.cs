using System.ComponentModel.DataAnnotations;

namespace Task_2EF.Models
{
    public class LoginStepTwoModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
    }
}
