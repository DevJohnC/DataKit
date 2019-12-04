using System;
using System.Linq;

namespace DataKit.Modelling
{
	/// <summary>
	/// Describes a field as a node on a graph.
	/// </summary>
	public class FieldGraphPath<TField>
		where TField : IModelField
	{
		public FieldGraphPath(string[] path, TField field)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
			Field = field;
		}

		public string[] Path { get; }

		public TField Field { get; }

		public DataType FieldType => Field.FieldType;

		public override bool Equals(object obj)
		{
			var cmp = obj as FieldGraphPath<TField>;
			if (cmp == null)
				return false;
			return Path.SequenceEqual(cmp.Path) && Field.Equals(cmp.Field);
		}
	}
}
