namespace DataKit.ORM.Schema.Sql
{
	public class JoinColumnPair
	{
		public string LeftColumnName { get; }
		public string RightColumnName { get; }

		public JoinColumnPair(string leftColumnName, string rightColumnName)
		{
			LeftColumnName = leftColumnName;
			RightColumnName = rightColumnName;
		}
	}
}
