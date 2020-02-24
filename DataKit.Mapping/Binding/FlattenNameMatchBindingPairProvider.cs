using System;
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
		private static readonly string[] _emptyArray = new string[0];

		public static FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField> Instance { get; }
			= new FlattenNameMatchBindingPairProvider<TSourceModel, TSourceField, TTargetModel, TTargetField>();

		/// <summary>
		/// Gets or sets a maximum depth to search for candidates.
		/// Defaults to 10.
		/// </summary>
		public int MaxDepth { get; set; } = 10;

		public IEnumerable<BindingPair<TSourceField, TTargetField>> GetBindingPairs(TSourceModel sourceModel, TTargetModel targetModel)
		{
			return ExpandModel(sourceModel, targetModel.Fields);

			IEnumerable<BindingPair<TSourceField, TTargetField>> ExpandModel(
				TSourceModel model, IReadOnlyList<TTargetField> candidateFields,
				string[] leftParentPath = null, string[] rightParentPath = null)
			{
				foreach (var sourceField in model.Fields)
				{
					foreach (var bindingPair in FindBindingPairs(sourceField, targetModel.Fields, leftParentPath, rightParentPath))
						yield return bindingPair;
				}
			}

			IEnumerable<BindingPair<TSourceField, TTargetField>> FindBindingPairs(
				TSourceField sourceField, IReadOnlyList<TTargetField> candidateFields,
				string[] leftParentPath = null, string[] rightParentPath = null)
			{
				if (leftParentPath == null)
					leftParentPath = _emptyArray;
				if (rightParentPath == null)
					rightParentPath = _emptyArray;

				if (leftParentPath.Length >= MaxDepth || rightParentPath.Length >= MaxDepth)
					yield break;

				var leftFieldPath = AppendToPath(leftParentPath, sourceField.FieldName);
				var leftFieldPathFlat = string.Concat(leftFieldPath);

				foreach (var candidateRightField in candidateFields)
				{
					var rightFieldPath = AppendToPath(rightParentPath, candidateRightField.FieldName);
					var rightFieldPathFlat = string.Concat(rightFieldPath);

					if (rightFieldPathFlat == leftFieldPathFlat)
					{
						yield return new BindingPair<TSourceField, TTargetField>(
							new ModelFieldBindingSource<TSourceField>(leftFieldPath, sourceField),
							new ModelFieldBindingTarget<TTargetField>(rightFieldPath, candidateRightField)
							);

						if (sourceField.FieldModel != null && !sourceField.FieldType.IsPureEnumerable)
						{
							foreach (var bindingPair in ExpandModel(sourceField.FieldModel, targetModel.Fields,
								leftFieldPath))
							{
								yield return bindingPair;
							}
						}
					}
					else if (candidateRightField.FieldModel != null && !sourceField.FieldType.IsPureEnumerable
						&& leftFieldPathFlat.StartsWith(rightFieldPathFlat, StringComparison.Ordinal))
					{
						foreach (var bindingPair in FindBindingPairs(sourceField, candidateRightField.FieldModel.Fields,
							leftParentPath, rightFieldPath))
						{
							yield return bindingPair;
						}
					}
				}
			}

			string[] AppendToPath(string[] path, string newSegment)
			{
				var ret = new string[path.Length + 1];
				Array.Copy(path, 0, ret, 0, path.Length);
				ret[path.Length] = newSegment;
				return ret;
			}
		}
	}
}
