using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace hmsBO
{

	public static class DBAccess
	{
        private const int TIMEOUT = 60;
		public static string[] EmptyKey = new string[] { "" };

		public static EventHandler<EventArgs> CaptureSQL;

		public static void Raise_CaptureSQL(string sql)
		{
			if (CaptureSQL != null)
			{
				CaptureSQL(sql, EventArgs.Empty);
			}
		}

		public static SqlConnection DBConnectionOpen()
		{
			string conStr = "dbConn_HMS";
            //string dbConStr = ConfigurationManager.ConnectionStrings[conStr].ConnectionString;
            string dbConStr = "data source=103.104.124.147;Initial Catalog=MEMRI_Dev;Persist Security Info=True;User ID=tsi;Password=t2EnNCKRkr;";
			SqlConnection cn = new SqlConnection(dbConStr);
			cn.Open();
			return cn;
		}

		public static void DBConnectionClose(SqlConnection cn)
		{
			try
			{
				if (cn != null)
				{
					if (cn.State != ConnectionState.Closed)
					{
						cn.Close();
					}
					cn = null;
				}
			}
			catch { }
		}
		public static void HandleSqlError(SqlException e)
		{

			StringBuilder sb = new StringBuilder();
			foreach (SqlError sqlError in e.Errors)
			{
				sb.Append(sqlError.ToString());
			}
			string exceptionInfo = String.Format("SqlException occurred.\n Details: \n{0}", sb.ToString());

			throw new Exception(exceptionInfo, e);
		}

		public static string sqlString(object arg)
		{
			string sql = "";
			if (arg == null)
				sql = "null";
			else if (arg == DBNull.Value)
				sql = "null";
			else if (arg is string)
				if (((string)arg).ToLower().Trim() == "(null)")
					sql = "null";
				else
					sql = "'" + ((string)arg).Replace("'", "''").Trim() + "'";
			else if (arg is DateTime)
				sql = "'" + ((DateTime)arg).ToString() + "'";
			else if (arg is Boolean)
				sql = Convert.ToInt16(arg).ToString();
			else if (arg is Guid)
				if ((Guid)arg == Guid.Empty)
					sql = "null";
				else
					sql = "'" + arg.ToString() + "'";
			else
				sql = arg.ToString().Trim();
			return sql;
		}
		public static string buildParams(params object[] args)
		{
			string sql = "";
			if (args.Length > 0)
			{
				foreach (object o in args)
				{
					sql += "," + sqlString(o);
				}
				sql = " " + sql.Substring(1);
			}
			return sql;
		}

		/// <summary>
		/// Executes a SQL command as a scalar and returns the 1st col in the 1st row of the result set
		/// </summary>
		/// <param name="sql">SQL Command to execute</param>
		/// <returns>SQL Return value</returns>
		public static object executeScalar(string sql, params object[] args)
		{
			string sql1 = sql + buildParams(args);
			Raise_CaptureSQL(sql1);
			object x = DBNull.Value;
			SqlConnection cn = null;
			try
			{
				cn = DBConnectionOpen();
				SqlCommand cmd = new SqlCommand(sql1, cn);
				x = cmd.ExecuteScalar();
			}
			catch (SqlException se)
			{
				throw se;
			}
			finally
			{
				DBConnectionClose(cn);
			}
			return x;
		}

		public static void executeSQL(string sql, params object[] args)
		{
			executeSQL(30, sql, args);
		}
		/// <summary>
		/// Executes a SQL command with no result set
		/// </summary>
		/// <param name="timeout">timeout in seconds</param>
		/// <param name="con">DB connection</param>
		/// <param name="sql">SQL Command to execute</param>
		public static void executeSQL(int timeout, string sql, params object[] args)
		{
			string sql1 = sql + buildParams(args);
			Raise_CaptureSQL(sql1);
			SqlConnection cn = null;
			try
			{
				cn = DBConnectionOpen();
				SqlCommand cmd = new SqlCommand(sql1, cn);
				cmd.CommandTimeout = timeout;
				cmd.ExecuteNonQuery();
			}
			catch (SqlException se)
			{
				throw se;
			}
			finally
			{
				DBConnectionClose(cn);
			}
		}

		//single table - DATASET
		//	--default timeout
		public static bool fillDS(DataSet ds, TableClass Table, string sp, params object[] args)
		{
			return fillDS(TIMEOUT, false, ds, new TableClass[] { Table }, sp, args);
		}
		public static bool fillDSnoIndex(DataSet ds, TableClass Table, string sp, params object[] args)
		{
			return fillDS(TIMEOUT, true, ds, new TableClass[] { Table }, sp, args);
		}
		//	--specified timeout
		public static bool fillDS(int timeout, DataSet ds, TableClass Table, string sp, params object[] args)
		{
			return fillDS(timeout, false, ds, new TableClass[] { Table }, sp, args);
		}
		public static bool fillDSnoIndex(int timeout, DataSet ds, TableClass Table, string sp, params object[] args)
		{
			return fillDS(timeout, true, ds, new TableClass[] { Table }, sp, args);
		}
		//multiple tables - DATASET
		//	--default timeout
		public static bool fillDS(DataSet ds, TableClass[] TableCollection, string sp, params object[] args)
		{
			return fillDS(TIMEOUT, false, ds, TableCollection, sp, args);
		}
		public static bool fillDSnoIndex(DataSet ds, TableClass[] TableCollection, string sp, params object[] args)
		{
			return fillDS(TIMEOUT, true, ds, TableCollection, sp, args);
		}
		//	--specific timeout
		public static bool fillDS(int timeout, DataSet ds, TableClass[] TableCollection, string sp, params object[] args)
		{
			return fillDS(timeout, false, ds, TableCollection, sp, args);
		}
		public static bool fillDSnoIndex(int timeout, DataSet ds, TableClass[] TableCollection, string sp, params object[] args)
		{
			return fillDS(timeout, true, ds, TableCollection, sp, args);
		}
		/// <summary>
		/// Add the result set from a SQL call to a dataset
		/// </summary>
		/// <param name="timeout">seconds to allow for SQL command timeout</param>
		/// <param name="noIndex">if true ignore table collection index parameters</param>
		/// <param name="ds">DataSet to add table too</param>
		/// <param name="TableCollection"></param>
		/// <param name="sp">SQL command</param>
		/// <param name="args">arguments to add to the SQL command</param>
		/// <returns></returns>
		public static bool fillDS(int timeout, bool noIndex, DataSet ds, TableClass[] TableCollection, string sp, params object[] args)
		{
			bool success = false;
			string errorInfo = "Initialize";
			if (sp.Length == 0 || TableCollection == null || TableCollection.Length == 0)
				return success;
			string sql = sp + buildParams(args);
			Raise_CaptureSQL(sql);
			int count = 0;
			string firstTableName = "";
			//clear previous tables
			foreach (TableClass table in TableCollection)
			{
				if (firstTableName == "")
				{
					//use the first table's name when filling the dataset
					firstTableName = table.TableName;
				}
				if (ds.Tables.Contains(table.TableName))
				{
					ds.Tables.Remove(table.TableName);
				}
			}

			SqlConnection cn = null;
			try
			{
				errorInfo = "Open DB connection";
				cn = DBConnectionOpen();
				SqlCommand cmd = new SqlCommand(sql, cn);
				cmd.CommandTimeout = timeout;
				errorInfo = "Create data adapter";
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				errorInfo = "Execute SQL (Fill)";
				da.Fill(ds, firstTableName);
				if (ds.Tables.Contains(firstTableName))
				{
					//rename tables and set primary keys
					int tableCount = 0;
					foreach (TableClass table in TableCollection)
					{
						//check for rename (no need to rename first table since we used it as the primary name)
						if (table.TableName != firstTableName && ds.Tables.Contains(firstTableName + tableCount.ToString()))
						{
							errorInfo = "Rename table " + firstTableName + tableCount.ToString() + " to " + table.TableName;
							ds.Tables[firstTableName + tableCount.ToString()].TableName = table.TableName;
						}
						//check for primary key to add
						count = 0;
						if (!noIndex && table.NumKeys > 0 && ds.Tables.Contains(table.TableName))
						{
							errorInfo = "Add primary key to " + table.TableName + " ";
							DataColumn[] keys = new DataColumn[table.NumKeys];
							foreach (string fld in table.KeyFields)
							{
								errorInfo += "[" + fld + "]";
								keys[count] = ds.Tables[table.TableName].Columns[fld];
								count++;
							}
							ds.Tables[table.TableName].PrimaryKey = keys;
						}
						//go to next table
						tableCount++;
					}
					success = true;
				}
				//tran.Commit();
			}
			catch (SqlException se)
			{
				se.Data.Add("DBAccess.FillDS", errorInfo);
				throw se;
			}
			catch (Exception e)
			{
				e.Data.Add("DBAccess.FillDS", errorInfo);
				throw e;
			}
			finally
			{
				DBConnectionClose(cn);
			}
			return success;
		}

		public static void AddKey(DataTable dt, params string[] fields)
		{
			DataColumn[] keys = new DataColumn[fields.Length];
			int count = 0;
			foreach (string fld in fields)
			{
				keys[count] = dt.Columns[fld];
				count++;
			}
			dt.PrimaryKey = keys;
		}

		public static void ClearTablesStartingWith(DataSet ds, string StartsWith)
		{
			foreach (DataTable dt in ds.Tables)
			{
				if (dt.TableName.StartsWith(StartsWith))
				{
					dt.Clear();
				}
			}
		}

		public static void AutoCreateParameterMappings(SqlCommand cmd)
		{
			foreach (SqlParameter param in cmd.Parameters)
			{
				if (param.SourceColumn == string.Empty)
				{
					param.SourceColumn = param.ParameterName.Replace("@", "");
				}
			}
		}
		public static void AutoCreateParameterMappings(SqlCommand cmd, DataRow dr)
		{
			string field = "";
			object obj = null;
			foreach (SqlParameter param in cmd.Parameters)
			{
				if (param.Direction == ParameterDirection.Input && param.Value == null)
				{
					field = param.ParameterName.Replace("@", "");
					obj = dr[field];
					param.Value = obj;

				}
			}
		}

		public static object[] AutoCreateParameterObjects(DataRow dr, params object[] startParms)
		{
			return AutoCreateParameterObjects("", dr, startParms);
		}
		/// <summary>
		/// Converts a DataRow into a series of objects
		/// </summary>
		/// <param name="cutOff">DataColumn to stop at</param>
		/// <param name="dr">DataRow to convert</param>
		/// <param name="startParms">If supplied a series of objects to prepend to the DataRow elements</param>
		/// <returns>Full list of objects</returns>
		public static object[] AutoCreateParameterObjects(string cutOff, DataRow dr, params object[] startParms)
		{
			int count, last = dr.ItemArray.Length;
			if (cutOff == null)
				cutOff = "";
			if (cutOff != "")
			{
				//preprocess the datarow so we know how many items will be in the final array
				for (count = 0; count < last; count++)
				{
					if (dr.Table.Columns[count].ColumnName == cutOff)
					{
						last = count + 1;
						break;
					}
				}
			}
			int place = 0;
			object[] final = new object[last + startParms.Length];
			if (startParms.Length > 0)
			{
				for (count = 0; count < startParms.Length; count++, place++)
				{
					final[place] = startParms[count];
				}
			}
			for (count = 0; count < last; count++, place++)
			{
				final[place] = dr.ItemArray[count];
			}
			return final;
		}

		public static DataRow FindFirstRow(DataTable dt, string field, object value)
		{
			DataRow ret = null;
			List<DataRow> drs = FindRow(dt, field, value);
			if (drs.Count > 0)
			{
				ret = drs[Utility.FIRST_ITEM];
			}
			return ret;
		}
		public static List<DataRow> FindRow(DataTable dt, string field, object value)
		{
			List<DataRow> drs = new List<DataRow>();
			if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains(field))
			{
				foreach (DataRow dr in dt.Rows)
				{
					if (dr[field].ToString() == value.ToString())
					{
						drs.Add(dr);
					}
				}
			}
			return drs;
		}
		public static SqlParameter CreateParameter(SqlCommand objCom, string paramName, SqlDbType dbType, object paramValue)
		{
			SqlParameter objParam = new SqlParameter(paramName, dbType);
			objParam.Value = paramValue;
			objCom.Parameters.Add(objParam);
			return objParam;
		}

		public static DataRow GetOnlyRow(DataSet ds, TableClass table)
		{
			return GetOnlyRow(ds, table.TableName);
		}
		/// <summary>
		/// Returns the only row from a given table.  If the table does not exist or is empty, null is returned.
		/// This method is functionally the same as GetFirstRow and is here for readability (i.e., you're expecting only 1 row in the table).
		/// </summary>
		/// <param name="ds"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static DataRow GetOnlyRow(DataSet ds, string tableName)
		{
			DataRow dr = null;
			if (ds != null && ds.Tables.Contains(tableName))
			{
				dr = GetOnlyRow(ds.Tables[tableName]);
			}
			return dr;
		}
		public static DataRow GetOnlyRow(DataTable dt)
		{
			DataRow dr = null;
			if (dt != null && dt.Rows.Count > 0)
			{
				dr = dt.Rows[Utility.ONLY_ITEM];
			}
			return dr;
		}

		public static DataRow GetFirstRow(DataSet ds, TableClass table)
		{
			return GetFirstRow(ds, table.TableName);
		}
		/// <summary>
		/// Returns the first row from a given table.  If the table does not exist or is empty, null is returned.
		/// </summary>
		/// <param name="ds"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static DataRow GetFirstRow(DataSet ds, string tableName)
		{
			DataRow dr = null;
			if (ds != null && ds.Tables.Contains(tableName))
			{
				dr = GetFirstRow(ds.Tables[tableName]);
			}
			return dr;
		}
		public static DataRow GetFirstRow(DataTable dt)
		{
			DataRow dr = null;
			if (dt != null && dt.Rows.Count > 0)
			{
				dr = dt.Rows[Utility.FIRST_ITEM];
			}
			return dr;
		}

		/// <summary>
		/// Method to return if a table in the dataset exists and has rows.
		/// </summary>
		public static bool HasRows(DataSet ds, TableClass table)
		{
			return HasRows(ds, table.TableName);
		}
		/// <summary>
		/// Method to return if a table in the dataset exists and has rows.
		/// </summary>
		public static bool HasRows(DataSet ds, string tableName)
		{
			return ds.Tables.Contains(tableName) && ds.Tables[tableName].Rows.Count > 0;
		}
		public static bool HasRows(DataTable tbl)
		{
			if (tbl != null && tbl.Rows.Count > 0)
				return true;
			return false;
		}
	}

	public class TableClass
	{
		private const int EMPTY = 0;	//used to set 0 length arrays

		public string TableName = "";
		public string[] KeyFields = new string[EMPTY];
		public string Description = "";

		public TableClass(string tableName, string keyField)
		{
			TableName = tableName;
			if (keyField.Length > 0)
			{
				KeyFields = new string[] { keyField };
			}
		}
		public TableClass(string tableName, string keyField, string description)
		{
			TableName = tableName;
			if (keyField.Length > 0)
			{
				KeyFields = new string[] { keyField };
			}
			Description = description;
		}
		public TableClass(string tableName, string[] keyFields)
		{
			TableName = tableName;
			if (keyFields != null)
			{
				KeyFields = keyFields;
			}
		}
		public TableClass(string tableName, string[] keyFields, string description)
		{
			TableName = tableName;
			if (keyFields != null)
			{
				KeyFields = keyFields;
			}
			Description = description;
		}

		public int NumKeys
		{
			get { return KeyFields.Length; }
		}

		public TableClass Clone(string newTableName)
		{
			TableClass newTable = new TableClass(newTableName, this.KeyFields, this.Description);

			return newTable;
		}
	}
}