using System.Data;

namespace hmsBO
{

	public static partial class TableDefs
	{
		public static class General
		{
			public enum Tables
			{
				tbl_cust			//customer
			}

			public static TableClass tbl_cust = new TableClass("tbl_cust", "cust_id");

			public static DataView GetCustomers(DataSet ds, bool includeInactives)
			{
				DBAccess.fillDS(ds, tbl_cust, "usp_cust_get_all", includeInactives);
				DataView dv = new DataView(ds.Tables[tbl_cust.TableName]);
				dv.Sort = "cust_name";
				return dv;
			}
			public static DataRow GetCustomer(DataSet ds, int cust_id)
			{
				DBAccess.fillDS(ds, tbl_cust, "usp_cust_get", cust_id);
				return DBAccess.GetOnlyRow(ds, tbl_cust);
			}
			
		}
	}
}