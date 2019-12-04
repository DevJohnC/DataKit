using System.Collections.Generic;
using System.Linq;
using DataKit.Modelling;

namespace DataKit.Mapping.Binding
{
	public class FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> : IBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceField : IModelField<TSourceModel, TSourceField>
		where TTargetField : IModelField<TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceModel, TSourceField>
		where TTargetModel : IDataModel<TTargetModel, TTargetField>
	{
		public static FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		/// <summary>
		/// Gets or sets a maximum depth to search for candidates.
		/// Defaults to 10.
		/// </summary>
		public int MaxDepth { get; set; } = 10;

		public IEnumerable<BindingPair<TSourceField, TTargetField>> GetBindingPairs(TSourceModel sourceModel, TTargetModel targetModel)
		{
			var flatSource = Flatten<TSourceModel, TSourceField>(sourceModel).ToArray();
			var flatTarget = Flatten<TTargetModel, TTargetField>(targetModel).ToArray();

			foreach (var sourceField in flatSource)
			{
				foreach (var targetField in flatTarget)
				{
					if (sourceField.FlatFieldPath == targetField.FlatFieldPath)
					{
						yield return new BindingPair<TSourceField, TTargetField>(
							new ModelFieldBindingSource<TSourceField>(sourceField.FieldPath, sourceField.Field),
							new ModelFieldBindingTarget<TTargetField>(targetField.FieldPath, targetField.Field)
							);
					}
				}
			}
		}

		private IEnumerable<FlatField<TModel, TField>> Flatten<TModel, TField>(TModel model)
			where TModel : IDataModel<TModel, TField>
			where TField : IModelField<TModel, TField>
		{
			foreach (var field in FlattenPath(model.Fields, new string[0], 0))
				yield return field;

			IEnumerable<FlatField<TModel, TField>> FlattenPath(
				IEnumerable<TField> fields,
				string[] path,
				int depth
				)
			{
				if (depth == MaxDepth)
					yield break;

				foreach (var field in fields)
				{
					var fieldPath = path.Concat(new string[] { field.FieldName }).ToArray();

					yield return new FlatField<TModel, TField>(
						string.Join("", fieldPath),
						fieldPath,
						model,
						field
						);

					if (field.FieldModel != null)
					{
						foreach (var subField in FlattenPath(field.FieldModel.Fields, fieldPath, depth + 1))
							yield return subField;
					}
				}
			}
		}

		private class FlatField<TModel, TField>
			where TModel : IDataModel<TModel, TField>
			where TField : IModelField<TModel, TField>
		{
			public string FlatFieldPath { get; }
			public string[] FieldPath { get; }
			public TModel Model { get; }
			public TField Field { get; }

			public FlatField(string flatFieldPath, string[] fieldPath,
				TModel model, TField field)
			{
				FlatFieldPath = flatFieldPath;
				FieldPath = fieldPath;
				Model = model;
				Field = field;
			}
		}
	}
}
