using System.Data;

namespace hmsBO
{

	public static partial class TableDefs
	{
		public static class User
		{
			public enum Tables
			{
				tbl_usr			//user
			}

			public static TableClass tbl_usr = new TableClass("tbl_usr", "tbl_usr");

			public static DataView GetUsers(DataSet ds, bool includeInactives) 
			{ 
				DBAccess.fillDS(ds, tbl_usr, "usp_usr_get_all", includeInactives); 
				DataView dv = new DataView(ds.Tables[tbl_usr.TableName]);
				dv.Sort = "usr_name";
				return dv;
			}
			public static DataRow GetUser(DataSet ds, int usr_id)
			{ 
				DBAccess.fillDS(ds, tbl_usr, "usp_usr_get", usr_id); 
				return DBAccess.GetOnlyRow(ds, tbl_usr); 
			}

			public static DataRow GetUserDetails(DataSet ds, string email)
			{
				DBAccess.fillDS(ds, tbl_usr, "usp_usr_getByemail", email);
				return DBAccess.GetOnlyRow(ds, tbl_usr);
			}
		}
	}
}