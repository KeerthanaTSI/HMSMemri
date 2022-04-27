using hmsBO;
using System;

namespace HMSMemri.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class LoginResponse
    {
        public UserClass userdetail { get; set; }
        public int Statuscode { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }

    }
    public class User {

        public int UserID { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public bool Authorized { get; set; }
    }

}
