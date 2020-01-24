using DataKit.Mapping.Binding;
using DataKit.Mapping.DelegateBuilding;
using DataKit.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Mapping
{
	public class MappingBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceField>
		where TTargetModel : IDataModel<TTargetField>
		where TSourceField : IModelField
		where TTargetField : IModelField
	{
		private readonly DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField> _binding;

		public MappingBuilder(DataModelBinding<TSourceModel, TSourceField, TTargetModel, TTargetField> binding)
		{
			_binding = binding;
		}

		private TSourceField[] GetSourceFieldsPath(ModelFieldBinding<TSourceField, TTargetField> boundFields)
		{
			var ret = new TSourceField[boundFields.BindingSource.Path.Length - 1];
			var fields = ((IDataModel)_binding.SourceModel).Fields;
			for (var i = 0; i < ret.Length; i++)
			{
				var field = (TSourceField)fields.FirstOrDefault(q => q.FieldName == boundFields.BindingSource.Path[i]);
				if (field == null)
					throw new Exception("Invalid field path.");
				ret[i] = field;
				fields = field.FieldModel.Fields;
			}
			return ret;
		}

		private TTargetField[] GetTargetFieldsPath(ModelFieldBinding<TSourceField, TTargetField> boundFields)
		{
			var ret = new TTargetField[boundFields.BindingTarget.Path.Length - 1];
			var fields = ((IDataModel)_binding.TargetModel).Fields;
			for (var i = 0; i < ret.Length; i++)
			{
				var field = (TTargetField)fields.FirstOrDefault(q => q.FieldName == boundFields.BindingTarget.Path[i]);
				if (field == null)
					throw new Exception("Invalid field path.");
				ret[i] = field;
				fields = field.FieldModel.Fields;
			}
			return ret;
		}

		public Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField> Build()
		{
			var delegateBuilder = new ExpressionBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>();

			foreach (var boundFields in SortBoundFieldsByPathAndDepth(_binding.FieldBindings))
			{
				var sourceBlock = delegateBuilder.StartSourceNavigateBlock(
					GetSourceFieldsPath(boundFields)
					);

				sourceBlock.WriteNavigateToReaderPositionInstruction(
					GetTargetFieldsPath(boundFields)
					);

				sourceBlock.WriteCopyValueInstruction(
					boundFields.SourceField,
					boundFields.TargetField,
					boundFields.Transformation
					);

				delegateBuilder.EndSourceNavigateBlock(sourceBlock);
			}

			return new Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField>(_binding.SourceModel, _binding.TargetModel, delegateBuilder.Build());
		}

		private IEnumerable<ModelFieldBinding<TSourceField, TTargetField>> SortBoundFieldsByPathAndDepth(
			IEnumerable<ModelFieldBinding<TSourceField, TTargetField>> fieldBindings
			)
		{
			return fieldBindings.OrderBy(
				q => string.Join("", q.BindingSource.Path)
				).ThenBy(
				q => q.BindingSource.Path.Length
				);
		}
	}
}
