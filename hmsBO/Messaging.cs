using System;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace hmsBO
{

    public class Messaging
    {

        private static string _sEmailServer = "";
        private static string _sEmailServerUser = "";
        private static string _sEmailServerPassword = "";
        private static string _sEmailFrom = "";
        private static string _sDevEmailTo = "";
        private static string _sProMode = "";
        private static int _sEmailServerPort = 0;
        private IConfiguration _iConfiguration;
        public static string sEmailServer
        {
            get { if (String.IsNullOrWhiteSpace(_sEmailServer)) { _sEmailServer = ConfigurationManager.AppSettings.Get("email_server"); } return _sEmailServer; }
            set { _sEmailServer = value; }
        }

        public static string sEmailServerUser
        {
            get { if (String.IsNullOrWhiteSpace(_sEmailServerUser)) { _sEmailServerUser = ConfigurationManager.AppSettings.Get("email_server_usr"); } return _sEmailServerUser; }
            set { _sEmailServerUser = value; }
        }

        public static string sEmailServerPassword
        {
            get { if (String.IsNullOrWhiteSpace(_sEmailServerPassword)) { _sEmailServerPassword = ConfigurationManager.AppSettings.Get("email_server_pwd"); } return _sEmailServerPassword; }
            set { _sEmailServerPassword = value; }
        }

        public static int sEmailServerPort
        {
            get { if (_sEmailServerPort == 0) { _sEmailServerPort = Utility.ToInt(ConfigurationManager.AppSettings.Get("email_server_port")); } return _sEmailServerPort; }
            set { _sEmailServerPort = value; }
        }

        public static string sEmailFrom
        {
            get { if (String.IsNullOrWhiteSpace(_sEmailFrom)) { _sEmailFrom = ConfigurationManager.AppSettings.Get("email_from"); } return _sEmailFrom; }
            set { _sEmailFrom = value; }
        }
        public static string sDevEmailTo
        {
            get { if (String.IsNullOrWhiteSpace(_sDevEmailTo)) { _sDevEmailTo = ConfigurationManager.AppSettings.Get("dev_email_to"); } return _sDevEmailTo; }
            set { _sDevEmailTo = value; }
        }
        public static string sProdMode
        {
            get { if (String.IsNullOrWhiteSpace(_sProMode)) { _sProMode = ConfigurationManager.AppSettings.Get("Mode"); } return _sProMode; }
            set { _sProMode = value; }
        }

        public static void SendHTMLEmail(IConfiguration _iConfiguration, string to, string subject, string body)
        {
            string sProdMode = _iConfiguration.GetSection("EmailService").GetSection("Mode").Value;
            string sEmailFrom = _iConfiguration.GetSection("EmailService").GetSection("email_from").Value;
            string sDevEmailTo = _iConfiguration.GetSection("EmailService").GetSection("dev_email_to").Value;
            string sEmailServer = _iConfiguration.GetSection("EmailService").GetSection("email_server").Value;
            string sEmailServerUser = _iConfiguration.GetSection("EmailService").GetSection("email_server_usr").Value;
            string sEmailServerPassword = _iConfiguration.GetSection("EmailService").GetSection("email_server_pwd").Value;
            int sEmailServerPort = Convert.ToInt32(_iConfiguration.GetSection("EmailService").GetSection("email_server_port").Value);

            //if (sProdMode == "0")
            //{
            //    to = sDevEmailTo;
            //}

            MailMessage mail = new MailMessage(sEmailFrom, to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(sEmailServer, sEmailServerPort);
            smtp.EnableSsl = true;
            NetworkCredential user = new NetworkCredential(sEmailServerUser, sEmailServerPassword);
            smtp.Credentials = user;
            smtp.SendCompleted += (s, e) =>
            {
                SmtpClient callbackClient = s as SmtpClient;
                MailMessage callbackMailMessage = e.UserState as MailMessage;
                callbackClient.Dispose();
                callbackMailMessage.Dispose();
            };
            try
            {
                smtp.SendAsync(mail, mail);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
    }
}