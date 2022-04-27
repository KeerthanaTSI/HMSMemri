using HMSMemri.BaseSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hmsBO;
using HMSMemri.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace HMSMemri.Controllers
{
    public class LoginController : IMCPBase
    {
        private readonly IOptions<BaseSettings.ServerVar> _serverVar;
        private IConfiguration _iConfiguration;
        public LoginController(IConfiguration iConfiguration ,IOptions<BaseSettings.ServerVar> serverVar) : base(serverVar)
        {
            _iConfiguration = iConfiguration;
            _serverVar = serverVar;
        }

        public IActionResult Index()
        {
            try
            {
                //HttpContext.Session.SetString("UserID", string.Empty);
                //HttpContext.Session.SetString("UserName", string.Empty);
                //HttpContext.Session.SetString("Password", string.Empty);
                //HttpContext.Session.Clear();

                UserClass userClass = new UserClass();
                userClass.SetData(10,"", true);
                Global = new GlobalData();
                Global.CurrentUser = userClass;
                //GlobalData.SetGlobalSessione(HttpContext.Session, _serverVar.Value.SessionGlobal, Global);
                return View();

            }
            catch (System.Exception ex)
            {

                throw;
            }
           
        }

        [HttpPost]
        public ActionResult SignIn(LoginModel request)
        {
            LoginResponse response = new LoginResponse();
            User user = new User();
            response.Statuscode = 0;
            Global = new GlobalData();
            try
            {
                Global.CurrentUser = Security.Login(request.UserName, request.Password, request.ErrorMessage);
                if(Global.CurrentUser.IsActive)
                {
                    //HttpContext.Session.SetString("UserID", Global.CurrentUser.UserID.ToString());
                    //HttpContext.Session.SetString("Email", Global.CurrentUser.UserName);
                    //HttpContext.Session.SetString("Password", request.Password);
                    //if (Global.CurrentUser.IsAuthorized(Rights.Application_Access))
                    //{
                    //    Global.CurrentUser.Authorized = true;
                    //}
                    //else
                    //{
                    //    response.ErrorMessage = "Invalid login.  Access logged.";
                    //}
                    response.userdetail = Global.CurrentUser; 
                    response.userdetail.UserID = Global.CurrentUser.UserID; 
                    response.userdetail.UserName = Global.CurrentUser.UserName; 
                    response.userdetail.Authorized = Global.CurrentUser.Authorized; 
                    response.userdetail.IsActive = Global.CurrentUser.IsActive; 
                    response.Statuscode = 1;
                    response.Status = "Success";
                }
                else
                {
                    response.Status = "Invalid login.  Access logged.";
                    response.Statuscode = 0;
                }               

            }
            catch (System.Exception ex)
            {
                response.Statuscode = 0;
                response.Status = "Failure";
                         
            }
            return Json(response);
        }


        [HttpPost]
        public ActionResult ForgetPassword(LoginModel request)
        {
            try
            {

                var test = Security.GetUserByEmail(request.Email);
                string new_pswd = Utility.GenerateTemporaryPswd();

                DBAccess.executeSQL("usp_usr_pswd_update", test.UserID, Utility.Encrypt(new_pswd), true /*reset_pswd*/);

                //send user email with new/temporary password
                StringBuilder sbBody = new StringBuilder();
                sbBody.AppendFormat("We received a request for password change for user {0} at HMS Health.<br>", request.Email);

                sbBody.AppendFormat("New Password: {0}", new_pswd);

                sbBody.Append("Regards,<br>");

                sbBody.Append("HMS Health");

                Messaging.SendHTMLEmail(_iConfiguration,request.Email, "HMS Health Password Reset", sbBody.ToString());
            }
            catch (System.Exception)
            {

                throw;
            }
            return Ok();
        }
    }
}
