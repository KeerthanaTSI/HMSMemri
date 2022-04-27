using hmsBO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace HMSMemri.BaseSettings
{
    public class GlobalData
    {
        public UserClass CurrentUser = new UserClass();
        //public string ErrorMessage { get; set; }
       // public UserClass user { get; set; }
        public static void SetGlobalSessione(Microsoft.AspNetCore.Http.ISession Session, string SessionGlobal, GlobalData Global)
        {
            Session.SetString(SessionGlobal, JsonConvert.SerializeObject(Global));
        }
    }
}
