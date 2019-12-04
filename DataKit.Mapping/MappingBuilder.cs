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

		public Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField> Build()
		{
			var delegateBuilder = new ExpressionBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>();

			var longestSourcePathLength = _binding.FieldBindings.Max(q => q.BindingSource.Path.Length);
			var longestTargetPathLength = _binding.FieldBindings.Max(q => q.BindingTarget.Path.Length);

			var currentSourcePath = new List<string>(longestSourcePathLength);
			var currentTargetPath = new List<string>(longestTargetPathLength);

			foreach (var boundFields in SortBoundFieldsByPathAndDepth(_binding.FieldBindings))
			{
				EnsureAtCorrectSourcePath(
					currentSourcePath,
					new Span<string>(boundFields.BindingSource.Path, 0, boundFields.BindingSource.Path.Length - 1),
					delegateBuilder);

				EnsureAtCorrectTargetPath(
					currentTargetPath,
					new Span<string>(boundFields.BindingTarget.Path, 0, boundFields.BindingTarget.Path.Length - 1),
					delegateBuilder);

				delegateBuilder.WriteCopyValueInstruction(
					boundFields.SourceField,
					boundFields.TargetField,
					boundFields.Transformation
					);
			}

			foreach (var pathSegment in currentSourcePath)
				delegateBuilder.WriteLeaveSourceMemberInstruction();
			foreach (var pathSegment in currentTargetPath)
				delegateBuilder.WriteLeaveTargetMemberInstruction();

			return new Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField>(_binding.SourceModel, _binding.TargetModel, delegateBuilder.Build());
		}

		private bool PathsEqual(List<string> currentPath, Span<string> desiredPath)
		{
			if (currentPath.Count != desiredPath.Length)
				return false;

			var i = 0;
			foreach (var currentPathSegment in currentPath)
			{
				if (currentPathSegment != desiredPath[i++])
					return false;
			}

			return true;
		}

		private int FindLastCommonPathIndex(List<string> currentPath, Span<string> desiredPath)
		{
			var i = 0;

			foreach (var currentPathSegment in currentPath)
			{
				if (desiredPath.Length < i + 1 || currentPathSegment != desiredPath[i])
					break;

				i++;
			}

			return i;
		}

		private void EnsureAtCorrectSourcePath(List<string> currentPath, Span<string> desiredPath,
			ExpressionBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> expressionBuilder)
		{
			if (PathsEqual(currentPath, desiredPath))
				return;

			var lastCommonIndex = FindLastCommonPathIndex(currentPath, desiredPath);

			while (lastCommonIndex < currentPath.Count)
			{
				//  step out
				currentPath.RemoveAt(currentPath.Count - 1);
				expressionBuilder.WriteLeaveSourceMemberInstruction();
			}

			while (currentPath.Count < desiredPath.Length)
			{
				//  step into
				var nextPathSegement = desiredPath[currentPath.Count];
				currentPath.Add(nextPathSegement);
				expressionBuilder.WriteEnterSourceMemberInstruction(GetSourceField(currentPath));
			}

			if (!PathsEqual(currentPath, desiredPath))
				throw new Exception("Failed to ensure correct source path.");
		}

		private IModelField GetSourceField(IEnumerable<string> path)
		{
			var field = default(IModelField);
			var fields = ((IDataModel)_binding.SourceModel).Fields;
			foreach (var fieldName in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == fieldName);
				if (field == null)
					throw new Exception("Invalid field path.");
				fields = field.FieldModel.Fields;
			}

			return field;
		}

		private IModelField GetTargetField(IEnumerable<string> path)
		{
			var field = default(IModelField);
			var fields = ((IDataModel)_binding.TargetModel).Fields;
			foreach (var fieldName in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == fieldName);
				if (field == null)
					throw new Exception("Invalid field path.");
				fields = field.FieldModel.Fields;
			}

			return field;
		}

		private void EnsureAtCorrectTargetPath(List<string> currentPath, Span<string> desiredPath,
			ExpressionBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField> expressionBuilder)
		{
			if (PathsEqual(currentPath, desiredPath))
				return;

			var lastCommonIndex = FindLastCommonPathIndex(currentPath, desiredPath);

			while (lastCommonIndex < currentPath.Count)
			{
				//  step out
				currentPath.RemoveAt(currentPath.Count - 1);
				expressionBuilder.WriteLeaveTargetMemberInstruction();
			}

			while (currentPath.Count < desiredPath.Length)
			{
				//  step into
				var nextPathSegement = desiredPath[currentPath.Count];
				currentPath.Add(nextPathSegement);
				expressionBuilder.WriteEnterTargetMemberInstruction(GetTargetField(currentPath));
			}

			if (!PathsEqual(currentPath, desiredPath))
				throw new Exception("Failed to ensure correct target path.");
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
