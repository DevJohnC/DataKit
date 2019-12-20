namespace DataKit.SQL.UnitTests
{
	public static class SqlStringHelpers
	{
		public static string ReduceWhitespace(this string str)
		{
			while (str.Contains("  "))
				str = str.Replace("  ", " ");
			while (str.Contains(" ,"))
				str = str.Replace(" ,", ",");
			while (str.Contains("( "))
				str = str.Replace("( ", "(");
			while (str.Contains(" )"))
				str = str.Replace(" )", ")");
			return str;
		}
	}
}
