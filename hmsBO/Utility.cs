using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace hmsBO
{
	public static class Utility
	{
		public const int ONLY_ITEM = 0;
		public const int FIRST_ITEM = 0;
		//this regex rules states: a string between 6 and 128 chars, no spaces, and must contain at least one uppercase, 
		//one lowercase and either a number or a symbol.
		public const string validPWRegEx = @"\A(?=\S*?[A-Z])(?=\S*?[a-z])((?=\S*?[0-9])|(?=\S*?[!@#$%^*_=+.-]))\S{6,128}\Z";
		public const string validEMChars = "@-_.0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		//Common RegEx strings
		public const string RegEx_None = "";	//used when no validation is desired - be sure to consiser using StripInjection() in conjunction with no regex validation
		//	Generic expressions
		public const string RegEx_CreditCard_Accepted = @"^3[47][0-9]{13}$|^4[0-9]{12}(?:[0-9]{3})?$|^5[1-5][0-9]{14}$|^6(?:011\d{12}|5\d{14}|4[4-9]\d{13}|22(?:1(?:2[6-9]|[3-9]\d)|[2-8]\d{2}|9(?:[01]\d|2[0-5]))\d{10})$";
		public const string RegEx_CreditCard_AmEx = @"^3[47][0-9]{13}$";
		public const string RegEx_CreditCard_Discover = @"^6(?:011\d{12}|5\d{14}|4[4-9]\d{13}|22(?:1(?:2[6-9]|[3-9]\d)|[2-8]\d{2}|9(?:[01]\d|2[0-5]))\d{10})$";
		public const string RegEx_CreditCard_MC = @"^5[1-5][0-9]{14}$";
		public const string RegEx_CreditCard_Visa = @"^4[0-9]{12}(?:[0-9]{3})?$";
		public const string RegEx_CreditCard_VerCode = @"^[0-9]{3,4}$";
		public const string RegEx_Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
		public const string RegEx_ItemDiv = @"^[\-\d]*$";
		public const string RegEx_OrderNumber = @"^[a-zA-Z0-9\-]*$";
		public const string RegEx_UPC = @"^[a-zA-Z0-9\-+]*$";
		public const string Regex_Pipe_Delimited_List = @"^[\w-\| \(\)]*$";
		//	Ecommerce site specific expressions
		public const string RegEx_Ecom_Account_FirstName = @"^[a-zA-Z'.\s]{1,15}$";
		public const string RegEx_Ecom_Account_LastName = @"^[a-zA-Z'.\s]{1,20}$";
		public const string RegEx_Ecom_Account_Company = @"^[a-zA-Z0-9'.\s]{1,30}$";
		public const string RegEx_Ecom_Address = @"^[a-zA-Z0-9'#/.\s]{1,40}$";
		public const string RegEx_Ecom_City = @"^[a-zA-Z'.\s]{1,40}$";
		public const string RegEx_Ecom_State = @"^[a-zA-Z'.\s]{1,40}$";
		public const string RegEx_Ecom_Zip_Canada = @"(^\d{5}(-\d{4})?$)|(^[A-Z]{1}\d{1}[A-Z]{1} *\d{1}[A-Z]{1}\d{1}$)";
		public const string RegEx_Ecom_Zip_US = @"^\d{5}(-\d{4})?$";
		public const string Const_Encrypt_Pswd = "C@rdin@lsAr3myT3@m";

		public static string AppSettings(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		public static string WherePart(string field, string keyword)
		{
			string escaped = keyword.Replace("'", "''").Replace("[", "[[]").Replace("_", "[_]").Replace("%", "[%]");
			return field + " like '%" + keyword + "%'";
		}

		public static string[] parseKeyWords(string search)
		{
			string[] results = null;
			if (search.Trim().Length > 0)
			{
				results = search.Split(' ');
			}
			return results;
		}

		public static string MD5Hash(string str)
		{
			byte[] originalBytes = ASCIIEncoding.Default.GetBytes(str);
			byte[] encodedBytes = new MD5CryptoServiceProvider().ComputeHash(originalBytes);
			return BitConverter.ToString(encodedBytes);
		}

		public static bool ContainsString(string[] array, string checkValue)
		{
			foreach (string item in array)
			{
				if (item.ToLower() == checkValue.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsCharacters(string strToCheck, string charsToLookFor)
		{
			if (strToCheck == null || strToCheck.Length == 0 || charsToLookFor == null || charsToLookFor.Length == 0)
			{
				return false;
			}
			bool found = false;
			if (strToCheck.IndexOfAny(charsToLookFor.ToCharArray()) >= 0)
			{
				found = true;
			}
			return found;
		}

		public static string StripInjection(string strToStrip)
		{
			return ReplaceIgnoreCase(ReplaceIgnoreCase(ReplaceIgnoreCase(ReplaceIgnoreCase(StripCharacters(strToStrip, @"<>"), "&lt;", ""), "&gt;", ""), "%3c", ""), "%3e", "").Replace("&#60;", "").Replace("&#62;", "");
		}

		public static string ReplaceIgnoreCase(string val, string pat, string rep)
		{
			return Regex.Replace(val, pat, rep, RegexOptions.IgnoreCase);
		}

		public static string StripCharacters(string strToStrip, string charsToStrip)
		{
			string newStr = strToStrip;
			if (newStr != null && newStr.Length > 0 && charsToStrip != null && charsToStrip.Length > 0)
			{
				foreach (char c in charsToStrip.ToCharArray())
				{
					newStr = newStr.Replace(c.ToString(), "");
				}
			}
			return newStr;
		}

		public static string StripPunctuation(string val)
		{
			var sb = new System.Text.StringBuilder();

			foreach (char c in val)
			{
				if (!char.IsPunctuation(c))
					sb.Append(c);
			}

			return val = sb.ToString();
		}

		/// <summary>
		/// Removes all non standard ASCII characters.  This includes all non-printable characters below ASCII 32 except Tab, LF, and CR as well
		/// as all characters above ASCII 126.
		/// </summary>
		/// <param name="strToClean">String to test and remove dissallowed characters from.</param>
		/// <returns>Cleaned string</returns>
		public static string CleanString(string strToClean)
		{
			StringBuilder sb = new StringBuilder();
			if (strToClean != null && strToClean.Length > 0)
			{
				char c;
				for (int x = 0; x < strToClean.Length; x++)
				{
					c = strToClean[x];
					if ((c >= ' ' && c <= '~') || c == '\t' || c == '\r' || c == '\n')
					{
						sb.Append(c);
					}
				}
			}
			return sb.ToString();
		}

		public static string ReverseString(string str)
		{
			if (str == null || str.Length == 0)
			{
				return String.Empty;
			}
			char[] arr = str.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}

		public static string FormatDollar(object val)
		{
			return FormatDollar(val, "0.00", false);
		}
		public static string FormatDollar(object val, bool addsign)
		{
			return FormatDollar(val, "0.00", addsign);
		}
		public static string FormatDollar(object val, string nullvalue, bool addsign)
		{
			string s = "";
			if (val == null || val == DBNull.Value || !IsNumeric(val))
			{
				s = nullvalue;
			}
			else
			{
				double d = ToDouble(val);
				s = (addsign ? "$" : "") + String.Format("{0:#,0.00}", d);
			}
			return s;
		}

		public static string ToShortDate(object val)
		{
			return FormatDateTime(val, "", "d");	// m/d/yyyy
		}
		public static string ToShortDateShortTime(object val)
		{
			return FormatDateTime(val, "", "g");	// m/d/yyyy hh:mm ?m
		}
		public static string ToShortDateLongTime(object val)
		{
			return FormatDateTime(val, "", "G");	// m/d/yyyy hh:mm:ss ?m
		}
		public static string ToLongDate(object val)
		{
			return FormatDateTime(val, "", "D");	// DayOfWeek, Month Day, Year
		}
		public static string ToLongDateShortTime(object val)
		{
			return FormatDateTime(val, "", "f");	// DayOfWeek, Month Day, Year hh:mm ?m
		}
		public static string ToLongDateLongTime(object val)
		{
			return FormatDateTime(val, "", "F");	// DayOfWeek, Month Day, Year hh:mm:ss ?m
		}
		public static string ToShortTime(object val)
		{
			return FormatDateTime(val, "", "t");	// hh:mm ?m
		}
		public static string ToLongTime(object val)
		{
			return FormatDateTime(val, "", "T");	// hh:mm:ss ?m
		}
		public static int ToCompareDate(object val)
		{
			string dt = FormatDateTime(val, "00000000", "yyyyMMdd");
			return Utility.ToInt(dt);
		}
		/// <summary>
		/// Returns a formatted datatime string based on the format
		/// </summary>
		/// <param name="val">DateTime variable</param>
		/// <param name="nullvalue">Value to return if 'val' is null or empty</param>
		/// <param name="format">DateTime format string:
		///		Standard formats-
		///		d			=	Short Date
		///		D			=	Long Date
		///		t			=	Short Time
		///		T			=	Long Time
		///		g			=	Short Date + Short Time
		///		G			=	Short Date + Long Time
		///		f			=	Long Date + Short Time
		///		F			=	Long Date + Long Time
		///		Custom formats-
		///		d			=	day (1-31)
		///		dd			=	day (01-31)
		///		ddd			=	abbr day name
		///		dddd		=	full day name
		///		f			=	fraction of a second (# of f's represent # of most significant digits shown
		///		h			=	hour (1-12)
		///		hh			=	hour (01-12)
		///		H			=	hour (0-23)
		///		HH			=	hour (00-23)
		///		m			=	minute (0-59)
		///		mm			=	minute (00-59)
		///		M			=	month (1-12)
		///		MM			=	month (01-12)
		///		MMM			=	abbr month name
		///		MMMM		=	full month name
		///		s			=	second (0-59)
		///		ss			=	second (00-59)
		///		t			=	A or P (AM/PM)
		///		tt			=	AM or PM
		///		yy			=	year (last 2 digits)
		///		yyyy		=	full year
		/// </param>
		/// <returns></returns>
		public static string FormatDateTime(object val, string nullvalue, string format)
		{
			string s = "";
			if (val == null || val == DBNull.Value || !IsDate(val))
			{
				s = nullvalue;
			}
			else
			{
				DateTime d = Convert.ToDateTime(val);
				s = d.ToString(format);
			}
			return s;
		}

		/// <summary>
		/// Returns a specified value from a connection string
		/// </summary>
		/// <param name="dbStr">DB Connection string to parse</param>
		/// <param name="param">parameter to find and return value for</param>
		/// <returns>parameter's value</returns>
		public static string SplitConnectionString(string dbStr, string param)
		{
			Regex matches = new Regex(@"\s*(.+?)\s*=\s*(.+?)\s*(?:;|$)");
			MatchCollection found = matches.Matches(dbStr);

			foreach (Match match in found)
			{
				if (match.Groups[1].Value == param)
					return match.Groups[2].Value;
			}
			return "(null)";
		}

		public static string ToUpperFirst(string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			// Return char and concat substring.
			return char.ToUpper(s[0]) + s.Substring(1);
		}
        #region File Operations
        // Simple synchronous file move operations with no user interface.
        public static void MoveFile(string sourceFullPath, string destinationPath)
        {
            string sourceFileName = Path.GetFileName(sourceFullPath);

            //verify destination directory exists, otherwise create
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // To move a file
            File.Move(sourceFullPath, destinationPath + "\\" + sourceFileName);

        }
        #endregion
        #region Search & Replace
        public static int IndexOf(string source, string pattern, int start, bool ignoreCase)
		{
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, pattern, start, (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None));
		}
		public static int LastIndexOf(string source, string pattern, int start, bool ignoreCase)
		{
			return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(source, pattern, start, (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None));
		}

		//--- Custom Replace Function
		public static string Replace(string strExpression, string strSearch, string strReplace, bool ignoreCase)
		{
			string strReturn = strExpression;
			if (ignoreCase)
			{
				int start = IndexOf(strReturn, strSearch, 0, true);
				string strTemp = "";
				while (start >= 0)
				{
					strTemp = strReturn.Substring(start, strSearch.Length);
					strReturn = strReturn.Replace(strTemp, "");
					start = IndexOf(strReturn, strSearch, 0, true);
				}
			}
			else
			{
				strReturn = strReturn.Replace(strSearch, strReplace);
			}
			return strReturn;

		}
		#endregion
		#region Data Validation
		public static bool IsEmpty(object val)
		{
			if (IsNull(val))
				return true;
			if (val.ToString().Trim().Length == 0)
				return true;
			return false;
		}
		public static bool IsNull(object val)
		{
			if (val == null)
				return true;
			if (val == DBNull.Value)
				return true;
			return false;
		}
		/// <summary>
		/// Tests whether an object could successfully be converted to a number
		/// </summary>
		/// <param name="val">Object to test</param>
		/// <returns>true if "val" can be converted to a number, otherwise false.</returns>
		public static bool IsNumeric(object val)
		{
			if (val == null)
				return false;
			if (val == DBNull.Value)
				return false;
			try
			{
				double d = System.Double.Parse(val.ToString(), System.Globalization.NumberStyles.Any);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		/// <summary>
		/// Tests whether an object could successfully be converted to a date
		/// </summary>
		/// <param name="val">Object to test</param>
		/// <returns>true if "val" can be converted to a date, otherwise false.</returns>
		public static bool IsDate(object val)
		{
			if (val == null)
				return false;
			if (val == DBNull.Value)
				return false;
			try
			{
				DateTime d = System.DateTime.Parse(val.ToString());
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		/// <summary>
		/// Tests "val" against the regular expression "regex".
		/// </summary>
		/// <param name="val">Value to validate.</param>
		/// <param name="regex">Regular expression pattern to match against "val".  If "regex" is null or empty, true is returned and no pattern matching is performed.</param>
		/// <returns></returns>
		public static bool MatchesRegex(string val, string regex)
		{
			bool valid = false;
			string x = ToString(val, "");
			if (regex == null || regex.Length == 0 || Regex.IsMatch(val, regex))
			{
				valid = true;
			}
			return valid;
		}

		public static bool IsValidPhoneNumber(string phone, out string errorMsg)
		{
			bool valid = true;
			errorMsg = "";
			string t = StripCharacters(phone, "-. ");
			if (t.Length != 10 || !IsNumeric(t))
			{
				valid = false;
				errorMsg = "Invalid phone number.  Use: ###-###-#### or ###.###.####";
			}
			return valid;
		}

		public static string FormatPhone(string number, bool forDisplay)
		{
			if(number != null)
            {
				number = number.Trim();
				if (number.Length < 10)
				{
					return number;
				}
				if (forDisplay)
				{
					number = number.Insert(3, ".");
					number = number.Insert(7, ".");
				}
				else
				{
					number = StripCharacters(number, "-. ");
				}
			}
			return number;
		}

		//Use to Format zip codes depending on whether they are going to or coming from DB, set forDisplay to true for values being displayed.
		public static string FormatZip(string zip, bool forDisplay)
		{

			zip = zip.Trim();
			if (forDisplay)
			{
				if (zip.Length < 6)
				{
					return zip;
				}
				else
				{
					zip = zip.Insert(5, "-");
					return zip;
				}
			}
			else
			{
				if (zip.Length < 6)
				{
					return zip;
				}
				else
				{
					if (zip.Contains("-"))
					{
						zip = zip.Remove(zip.IndexOf('-'), 1);
					}
					else if (zip.Contains("+"))
					{
						zip = zip.Remove(zip.IndexOf('+'), 1);
					}
					else if (zip.Contains(" "))
					{
						zip = zip.Remove(zip.IndexOf(' '), 1);
					}
					return zip;
				}
			}

		}

		/// <summary>
		/// Checks for a valid email.  Does not allow Roeslein email addresses.
		/// </summary>
		public static bool IsValidEmail(string email, out string errorMsg)
		{
			return IsValidEmail(email, out errorMsg, false);
		}

		/// <summary>
		/// Checks for a valid email.
		/// </summary>
		public static bool IsValidEmail(string email, out string errorMsg, bool allowRoeslein)
		{
			bool valid = true;
			errorMsg = "";
			if (!email.Contains("@") || !email.Contains(".") || email.Length < 6)
			{
				errorMsg = "E-mail must contain an @ sign, a '.' (period), and be at least 6 characters long.";
				valid = false;
			}
			else if (email.IndexOf('@') != email.LastIndexOf('@'))
			{
				errorMsg = "E-mail address can only contain a single @ sign.";
				valid = false;
			}
			else if (email.LastIndexOf('.') < email.IndexOf('@') || email.Length - email.LastIndexOf('.') < 3)
			{
				errorMsg = "E-mail domain does not appear to be valid.";
				valid = false;
			}
			else if (!allowRoeslein && email.ToLower().Contains("roeslein"))
			{
				errorMsg = "'Roeslein' is reserved for Roeslein employees and cannot appear anywhere in an e-mail address.";
				valid = false;
			}
			else
			{
				//test for invalid characters if we got this far
				errorMsg = "E-mail contains invalid characters: ";
				string c = "";
				for (int x = 0; x < email.Length; x++)
				{
					if (!validEMChars.Contains(email.Substring(x, 1)))
					{
						if (!c.Contains(email.Substring(x, 1)))
						{
							c += email.Substring(x, 1);
							if (email.Substring(x, 1) == " ")
							{
								errorMsg += "(space) ";
							}
							else
							{
								errorMsg += email.Substring(x, 1) + " ";
							}
						}
						valid = false;
					}
				}
				if (valid)
				{
					//reset error message
					errorMsg = "";
					//test the domain further
					string domain = email.Substring(email.IndexOf('@') + 1);
					//remove previously allowed symbols which are not valid for the domain portion of an e-mail address
					string cleaned_domain = StripCharacters(domain, "@+_");		// . and - are the only symbols allowed in domain names
					if (domain != cleaned_domain)
					{
						errorMsg = "The e-mail domain contains invalid characters.";
						valid = false;
					}
					else
					{
						//reset for periods even though in theory can't get here without at least 1
						if (cleaned_domain.Contains("."))
						{
							string[] parts = cleaned_domain.Split('.');
							foreach (string s in parts)
							{
								if (s.Length == 0 || s.StartsWith("-") || s.EndsWith("-"))
								{
									errorMsg = "Invalid e-mail domain.";
									valid = false;
								}
							}
							//final check of e-mail domain type (last part of domain)
							if (valid)
							{
								//get last piece of domain
								string domain_type = parts[parts.Length - 1];
								if (domain_type.Length < 2 || domain_type != StripCharacters(domain_type, "-1234567890"))
								{
									errorMsg = "Invalid e-mail domain type (should be com, net, org, edu, country code, etc.)";
									valid = false;
								}
							}
						}
						else
						{
							errorMsg = "Invalid e-mail domain.";
							valid = false;
						}
					}
				}
			}
			return valid;
		}

		/// <summary>
		/// Validates a password.
		/// </summary>
		/// <param name="errMsg">User Friendly error message</param>
		public static bool IsValidPassword(string pswd, out string errMsg)
		{
			bool valid = true;
			errMsg = String.Empty;

			//validate password
			try
			{
				valid = Regex.IsMatch(pswd, validPWRegEx);
			}
			catch (ArgumentException ex)
			{
			    valid = false;
			}

			//handle invalid password
			if (!valid)
			{
			    errMsg = "The password must be at least 6 characters, can contain no spaces, and must" +
					" include at least one uppercase, one lowercase, and either a number or a symbol.";
			}

			return valid;
		}
		#endregion

		#region Data Conversion
		public static Guid ToGuid(object val)
		{
			Guid rtn = Guid.Empty;
			try
			{
				if (val != null && val != DBNull.Value)
				{
					if (val is Guid)
					{
						rtn = (Guid)val;
					}
					else if (val is string)
					{
						rtn = new Guid((string)val);
					}
					else if (val is byte[])
					{
						rtn = new Guid((byte[])val);
					}
				}
			}
			catch
			{
			}
			return rtn;
		}

		/// <summary>
		/// Converts a string into a database string object.
		/// If the string is empty or contains "(null)" then DBNull is automatically assigned.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static object ToDBString(string val)
		{
			if (val == null)
				return DBNull.Value;
			if (val.Trim().Length == 0)
				return DBNull.Value;
			if (val.Trim().ToLower() == "(null)")
				return DBNull.Value;
			return (object)val.Trim();
		}
		/// <summary>
		/// Concerts a string into a database integer object.
		/// If the string is empty or contains "(null)" then DBNull is automatically assigned.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static object ToDBInt(string val)
		{
			if (val == null)
				return DBNull.Value;
			if (val.Trim().Length == 0)
				return DBNull.Value;
			if (val.Trim().ToLower() == "(null)")
				return DBNull.Value;
			if (!IsNumeric(val.Trim()))
				throw new Exception("(ToDBInt) Can not convert string to a number: " + val);
			return (object)ToInt(val.Trim());
		}
		/// <summary>
		/// Converts a string into a database datetime object.
		/// If the string is empty or contains "(null)" then DBNull is automatically assigned.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static object ToDBDateTime(string val)
		{
			if (val == null)
				return DBNull.Value;
			if (val.Trim().Length == 0)
				return DBNull.Value;
			if (val.Trim().ToLower() == "(null)")
				return DBNull.Value;
			if (!IsDate(val))
				throw new Exception("(ToDBDateTime) Invalid date supplied: " + val);
			return Convert.ToDateTime(val);
		}
		/// <summary>
		/// Convert object to a string, return "(null)" if object does not contain data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>string result</returns>
		public static string ToString(object val)
		{
			return ToString(val, "(null)");
		}
		public static string ToString(object val, string nullValue)
		{
			return ToString(val, nullValue, false);
		}
		public static string ToString(object val, bool emptyIsNull)
		{
			return ToString(val, "(null)", emptyIsNull);
		}
		public static string ToString(object val, string nullValue, bool emptyIsNull)
		{
			string rtn = nullValue;
			if (val == null)
				return rtn;
			if (val == DBNull.Value)
				return rtn;
			if (val is string)
				rtn = (string)val;
			else
			{
				try
				{
					rtn = Convert.ToString(val);
				}
				catch
				{
					return nullValue;
				}
			}
			if (emptyIsNull && rtn.Trim().Length == 0)
				return nullValue;
			return rtn;
		}
		public static string ToEmptyString(object val)
		{
			return ToString(val, "");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <param name="error_val"></param>
		/// <returns></returns>
		public static bool ToBool(object val, bool error_val)
		{
			bool rtn = false;
			if (val != null && val != DBNull.Value)
			{
				try
				{
					if (val is bool)
					{
						rtn = (bool)val;
					}
					else if (val is string)
					{
						string s = ToString(val);
						rtn = false;
						if (s != "" && s != "0" && s.ToLower() != "false")
						{
							rtn = true;
						}

					}
					else
					{
						rtn = Convert.ToBoolean(val);
					}
				}
				catch
				{
					rtn = error_val;
				}
			}
			return rtn;
		}

		/// <summary>
		/// Convert object to an Int32, return 0 if object does not contain numeric data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>Int32 number</returns>
		public static int ToInt(object val)
		{
			if (val == null || val == DBNull.Value)
			{
				return 0;
			}
			if (val is bool)
			{
				if ((bool)val)
					return 1;
				else
					return 0;
			}
			val = CleanNumber(val);
			if (IsNumeric(val))
			{
				try
				{
					//Convert.ToInt32 performs rounding; instead convert to double then return the integer part
					double x = Convert.ToDouble(val);
					return (int)x;
				}
				catch
				{
					return 0;
				}
			}
			else
				return 0;
		}

		/// <summary>
		/// Convert object to an Int64/BigInt, return 0 if object does not contain numeric data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>Int64 number</returns>
		public static long ToLong(object val)
		{
			if (val == null || val == DBNull.Value)
			{
				return 0;
			}
			if (val is bool)
			{
				if ((bool)val)
					return 1;
				else
					return 0;
			}
			val = CleanNumber(val);
			if (IsNumeric(val))
			{
				try
				{
					//Convert.ToInt32 performs rounding; instead convert to double then return the integer part
					double x = Convert.ToDouble(val);
					return (long)x;
				}
				catch
				{
					return 0;
				}
			}
			else
				return 0;
		}

		/// <summary>
		/// Convert object to a decimal, return 0 if object does not contain numeric data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>decimal number</returns>
		public static decimal ToDecimal(object val)
		{
			val = CleanNumber(val);
			if (IsNumeric(val))
				return Convert.ToDecimal(val);
			else
				return 0M;
		}

		/// <summary>
		/// Convert object to a double, return 0 if object does not contain numeric data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>double precision number</returns>
		public static double ToDouble(object val)
		{
			val = CleanNumber(val);
			if (IsNumeric(val))
				return Convert.ToDouble(val);
			else
				return 0;
		}

		/// <summary>
		/// Convert object to a float, return 0 if object does not contain numeric data
		/// </summary>
		/// <param name="val">Object to convert</param>
		/// <returns>single precision number</returns>
		public static float ToFloat(object val)
		{
			val = CleanNumber(val);
			if (IsNumeric(val))
				return Convert.ToSingle(val);
			else
				return 0;
		}
		public static object CleanNumber(object val)
		{
			if (val != null && val != DBNull.Value && val is string)
			{
				string s = val.ToString();
				s = s.Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", "").Trim();
				//the isnumeric function will return true for strings that have a minus sign anywhere in the string even though an attempt to convert the string
				//	to a number will fail if the minus sign is anywhere but the beginning
				if (s.IndexOf('-') > 0)
				{
					//replace "-" with something that will force a fail
					s = s.Replace("-", "a");
				}
				val = s;
			}
			return val;
		}
        #endregion
        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="password">Password</param>
        /// 
        #region old code
        //public static string Encrypt(string plainText)
        //{
        //	if (plainText == null)
        //	{
        //		return null;
        //	}

        //	//if (password == null)
        //	//{
        //	//    password = String.Empty;
        //	//}

        //	string password = Const_Encrypt_Pswd;

        //	// Get the bytes of the string
        //	var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
        //	var passwordBytes = Encoding.UTF8.GetBytes(password);

        //	// Hash the password with SHA256
        //	passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        //	var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

        //	return Convert.ToBase64String(bytesEncrypted);
        //}


        //private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        //{
        //	byte[] encryptedBytes = null;

        //	// Set your salt here, change it to meet your flavor:
        //	// The salt bytes must be at least 8 bytes.
        //	var saltBytes = new byte[] { 36, 6, 47, 82, 80, 54, 1, 65 };

        //	using (MemoryStream ms = new MemoryStream())
        //	{
        //		using (RijndaelManaged AES = new RijndaelManaged())
        //		{
        //			var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

        //			AES.KeySize = 256;
        //			AES.BlockSize = 128;
        //			AES.Key = key.GetBytes(AES.KeySize / 8);
        //			AES.IV = key.GetBytes(AES.BlockSize / 8);

        //			AES.Mode = CipherMode.CBC;

        //			using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
        //			{
        //				cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
        //				cs.Close();
        //			}

        //			encryptedBytes = ms.ToArray();
        //		}
        //	}

        //	return encryptedBytes;
        //}
        #endregion

        public static string GenerateTemporaryPswd()

		{

			return Guid.NewGuid().ToString("N").ToLower().Replace("1", "").Replace("o", "").Replace("0", "").Substring(0, 10);

		}
		public static string Encrypt(string plainText)

		{
			if (plainText == null)
			{
				return null;
			}

			string password = Const_Encrypt_Pswd;

			// Get the bytes of the string

			var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
			var passwordBytes = Encoding.UTF8.GetBytes(password);

			// Hash the password with SHA256

			passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
			var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

			return Convert.ToBase64String(bytesEncrypted);

		}
		private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
		{

			byte[] encryptedBytes = null;

			// Set your salt here, change it to meet your flavor:

			// The salt bytes must be at least 8 bytes.

			var saltBytes = new byte[] { 36, 6, 47, 82, 80, 54, 1, 65 };

			using (MemoryStream ms = new MemoryStream())
			{
				using (RijndaelManaged AES = new RijndaelManaged())
				{
					var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

					AES.KeySize = 256;
					AES.BlockSize = 128;
					AES.Key = key.GetBytes(AES.KeySize / 8);
					AES.IV = key.GetBytes(AES.BlockSize / 8);
					AES.Mode = CipherMode.CBC;

					using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
						cs.Close();
					}
					encryptedBytes = ms.ToArray();
				}
			}
			return encryptedBytes;

		}
	}
}
