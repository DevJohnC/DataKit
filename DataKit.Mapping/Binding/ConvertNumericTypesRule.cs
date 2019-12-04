using DataKit.Modelling;
using System;
using System.Linq;

namespace DataKit.Mapping.Binding
{
	public class ConvertNumericTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingRule<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
	{
		public static ConvertNumericTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new ConvertNumericTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		private static Type[] _numericTypes = new[]
		{
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(decimal),
			typeof(float),
			typeof(double)
		};

		private static bool IsNumericType(Type type)
		{
			return type.IsEnum || _numericTypes.Contains(type);
		}

		public void AddBindings(DataModelBindingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> builder, BindingPair<TSourceField, TTargetField> bindingPair)
		{
			if (!IsNumericType(bindingPair.Source.FieldType.Type) ||
				!IsNumericType(bindingPair.Target.FieldType.Type) ||
				builder.IsBound(bindingPair.Target))
				return;

			//  bind with just a copy operation
			//  since the transform APIs work with objects and box integers etc.
			//  a cast is required in consuming APIs, thus, they handle the cast for us
			builder.Bind(bindingPair.Target, bindingPair.Source,
				CopyIdenticalValueTypesRule<TSourceModel, TSourceField, TTargetModel, TTargetField>.ValueCopyTransformFactory.Instance);
		}
	}
}
