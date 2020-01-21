using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataKit.Modelling.TypeModels
{
	public abstract class TypeModel : IDataModel<TypeModel, PropertyField>
	{
		private readonly Dictionary<string, PropertyField> _fields
			= new Dictionary<string, PropertyField>();

		public abstract DataType Type { get; }

		public IReadOnlyList<PropertyField> Fields { get; }

		IReadOnlyList<IModelField> IDataModel.Fields => Fields;

		IModelField IDataModel.this[string fieldName] => _fields[fieldName];

		public PropertyField this[string fieldName] => _fields[fieldName];

		internal TypeModel(IReadOnlyList<PropertyField> fields)
		{
			Fields = fields;
			foreach (var field in fields)
				// in lieu of support of multiple fields with the same name we just go
				// with the last one in the list which should be the most derived
				_fields[field.FieldName] = field;
		}

		private static readonly CachedCollection<Type, Type, TypeModel> _cachedCollection =
			new CachedCollection<Type, Type, TypeModel>(TypeModelFactory.BuildTypeModel);

		public static TypeModel GetModelOf(Type type)
		{
			return _cachedCollection.GetOrCreate(type, type);
		}

		public static TypeModel<T> GetModelOf<T>()
			=> GetModelOf(typeof(T)) as TypeModel<T>;

		public bool TryGetField(string fieldName, out PropertyField field)
			=> _fields.TryGetValue(fieldName, out field);

		public bool TryGetField(string fieldName, out IModelField field)
		{
			if (_fields.TryGetValue(fieldName, out var outField))
			{
				field = outField;
				return true;
			}
			field = null;
			return false;
		}

		public IModelNode Accept(IModelVisitor visitor)
		{
			return visitor.VisitModel(this);
		}
	}

	public class TypeModel<T> : TypeModel
	{
		public override DataType Type { get; } = DataType.GetTypeOf<T>();

		public TypeModel(IReadOnlyList<PropertyField> fields) :
			base(fields)
		{
		}

		private PropertyField GetField(IEnumerable<string> path)
		{
			var fields = Fields;
			var result = default(PropertyField);

			foreach (var segment in path)
			{
				if (fields == null)
					throw new InvalidOperationException("Missing sub-model in expression path.");

				result = fields.FirstOrDefault(q => q.FieldName == segment);
				if (result == null)
					throw new InvalidOperationException("Missing field in expression path.");

				fields = result.FieldModel?.Fields;
			}

			return result;
		}

		public FieldGraphPath<PropertyField> GetField<TValue>(Expression<Func<T, TValue>> expression)
		{
			var node = expression.Body as MemberExpression;
			var path = new List<string>();

			while (true)
			{
				if (node == null || !(node.Member is PropertyInfo))
					throw new InvalidOperationException("Expression must be comprised of property expressions.");

				path.Insert(0, node.Member.Name);

				if (node.Expression == null || node.Expression == expression.Parameters[0])
					return new FieldGraphPath<PropertyField>(
						path.ToArray(), GetField(path));

				node = node.Expression as MemberExpression;
			}
		}
	}
}
