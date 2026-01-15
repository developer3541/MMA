using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public class LoginDto
    {
        //[Required]
        public string UserName { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        public string Password { get; set; }
    }
}