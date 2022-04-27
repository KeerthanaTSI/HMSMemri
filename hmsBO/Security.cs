using System;
using System.Data;
using System.DirectoryServices.Protocols;
using System.Net;

namespace hmsBO
{
	public enum Authorization { LoginRequired = -1, NotAuthorized = 0, Authorized = 1 }

	public enum Rights
	{
		None = 0,                              //no right needed
		Application_Administrator = 1,         //full access, ignores all other security rights
		Application_Access = 2                 //Project web appliation access
	}

	public enum LogType { Invalid = -1, Info = 0, Error = 1, Success = 2 }
	/// <summary>
	/// Summary description for security.
	/// </summary>
	public static class Security
	{
		public static string LOGIN_REQUIRED = "javascript:alert('Please log in to use this feature.');";
		public static string NOT_AUTHORIZED = "javascript:alert('You are not authorized for this area.');";

		public static UserClass Login(string userLogin, string userPswd, string ErrorMsg)
		{
			UserClass user = new UserClass();
			DataSet ds = new DataSet();
			ErrorMsg = String.Empty;
			string LogMsg = String.Empty;


			string IP = "";
			string remote_host = "";
			string user_agent = "";


			if (userLogin == null || userPswd == null)
			{
				ErrorMsg = "Invalid login.  Access logged.";
			}
			else
			{
				userLogin = userLogin.Replace("\"", "").Replace("'", "").Replace("-", "").Trim();
				userPswd = userPswd.Replace("\"", "").Replace("'", "");

				if (userLogin.Length == 0 || userPswd.Length == 0)
				{
					ErrorMsg = "Invalid login.  Access logged.";
				}
				else
				{
					try
					{
						DBAccess.fillDS(ds, TableDefs.User.tbl_usr, "usp_usr_login", userLogin);
						if (ds.Tables.Contains(TableDefs.User.tbl_usr.TableName) && ds.Tables[TableDefs.User.tbl_usr.TableName].Rows.Count > 0)
						{
							DataRow drUser = ds.Tables[TableDefs.User.tbl_usr.TableName].Rows[0];
							string dbPswd = drUser["usr_pswd"].ToString();
							string encryptPswd = userPswd; // Utility.Encrypt(userPswd);
							if (dbPswd == encryptPswd)
							{
								user.SetData((int)drUser["usr_id"], (string)drUser["usr_name"], Utility.ToBool(drUser["is_active"], false));
								if (!Utility.ToBool(drUser["is_active"], false))
								{
									ErrorMsg = "Invalid login.  Access logged.";
									LogMsg = "Inactive user";
								}
							}
							else
							{
								ErrorMsg = "Invalid login.  Access logged.";
								LogMsg = "Incorrect password";
							}
						}
						else
						{
							ErrorMsg = "Invalid login.  Access logged.";
							LogMsg = "User not found";
						}
					}
					catch (Exception ex)
					{
						ErrorMsg = ex.Message + "  Access logged.";
						LogMsg = ErrorMsg;
					}
				}
			}
			if (ErrorMsg == String.Empty)
			{
				//log success
				LogEntry(remote_host, user_agent, IP, userLogin, user.UserID, LogType.Success, "login", "");
			}
			else
			{
				//log failure
				LogEntry(remote_host, user_agent, IP, userLogin, user.UserID, LogType.Invalid, "login", LogMsg);
			}
			return user;
		}

        public static void LogEntry(string remote_host, string user_agent, string IP, string login, UserClass User, LogType logType, string action, string notes)
		{
			LogEntry(remote_host, user_agent, IP, login, User.UserID, logType, action, notes);
		}

		public static void LogEntry(string remote_host, string user_agent, string IP, string login, int usr_id,
			LogType logType, string action, string notes)
		{
			DBAccess.executeSQL("usp_usr_trk_log_entry", remote_host, user_agent, IP, login, usr_id, (int)logType, action, notes);
		}



		public static UserClass GetUserByEmail(string email)
        {
			UserClass user = new UserClass();
			DataSet ds = new DataSet();
			string LogMsg = String.Empty;
            try
            {

				DBAccess.fillDS(ds, TableDefs.User.tbl_usr, "usp_usr_getByemail", email);				
				if (DBAccess.GetOnlyRow(ds, TableDefs.User.tbl_usr) != null)
				{
					DataRow drUser = DBAccess.GetOnlyRow(ds, TableDefs.User.tbl_usr);
					user.SetData((int)drUser["usr_id"], (string)drUser["usr_name"], Utility.ToBool(drUser["is_active"], false));

				}
				else
				{					
					LogMsg = "User not found";
				}
			}
            catch (Exception)
            {

                throw;
            }
			return user;

		}

	}
	public class UserClass
	{
		public int UserID = 0;
		public string UserName = "";
		public bool IsActive = false;
		public bool Authorized = false;
		
		public UserClass()
		{
		}

		public void SetData(int userID, string userName, bool isActive)
		{
			UserID = userID;
			UserName = userName;
			IsActive = isActive;
		}

		/// <summary>
		/// Used to log someone out
		/// </summary>
		public void Reset()
		{
			UserID = 0;
			UserName = "";
			IsActive = false;
			Authorized = false;
		}

		public bool IsAuthorized(Rights CheckRight)
		{
			if (CheckRight == Rights.None)
			{
				return true;
			}
			return (bool)DBAccess.executeScalar("usp_usr_chk_right", UserID, (int)CheckRight);
		}
	}
}
