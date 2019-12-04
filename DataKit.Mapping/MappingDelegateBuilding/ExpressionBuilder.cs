using DataKit.Mapping.Binding;
using DataKit.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.Mapping.DelegateBuilding
{
	public class ExpressionBuilder<TSourceModel, TSourceField, TTargetModel, TTargetField>
		where TSourceModel : IDataModel<TSourceField>
		where TSourceField : IModelField
		where TTargetModel : IDataModel<TTargetField>
		where TTargetField : IModelField
	{
		private static readonly Type _dataModelReaderType = typeof(IDataModelReader<TSourceModel, TSourceField>);

		private static readonly Type _dataModelWriterType = typeof(IDataModelWriter<TTargetModel, TTargetField>);

		private readonly List<Expression> _expressionBody = new List<Expression>();

		private readonly ParameterExpression _bindingContext = Expression.Variable(typeof(BindingContext), "bindingContext");

		private readonly ParameterExpression _sourceReader = Expression.Parameter(_dataModelReaderType, "sourceReader");

		private readonly ParameterExpression _targetWriter = Expression.Parameter(_dataModelWriterType, "targetWriter");

		public void WriteEnterSourceMemberInstruction(IModelField source)
		{
			var sourceField = Expression.Constant(source, typeof(TSourceField));
			var enterMemberMethod = _dataModelReaderType.GetMethod(nameof(IDataModelReader<TSourceModel, TSourceField>.EnterMember));

			_expressionBody.Add(
				Expression.Call(_sourceReader, enterMemberMethod, sourceField)
				);
		}

		public void WriteLeaveSourceMemberInstruction()
		{
			var leaveMemberMethod = _dataModelReaderType.GetMethod(nameof(IDataModelReader<TSourceModel, TSourceField>.LeaveMember));

			_expressionBody.Add(
				Expression.Call(_sourceReader, leaveMemberMethod)
				);
		}

		public void WriteEnterTargetMemberInstruction(IModelField target)
		{
			var targetField = Expression.Constant(target, typeof(TTargetField));
			var enterMemberMethod = _dataModelWriterType.GetMethod(nameof(IDataModelWriter<TTargetModel, TTargetField>.EnterMember));

			_expressionBody.Add(
				Expression.Call(_targetWriter, enterMemberMethod, targetField)
				);
		}

		public void WriteLeaveTargetMemberInstruction()
		{
			var leaveMemberMethod = _dataModelWriterType.GetMethod(nameof(IDataModelWriter<TTargetModel, TTargetField>.LeaveMember));

			_expressionBody.Add(
				Expression.Call(_targetWriter, leaveMemberMethod)
				);
		}

		/// <summary>
		/// Write an instruction to copy a value from the source to the target using the provided binding transformation.
		/// </summary>
		/// <remarks>
		/// Both the sourceReader and targetWriter parameters must be advanced to the correct state for reading/writing.
		/// This method will not travel into the read/write graph.
		/// </remarks>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="transformationDelegate"></param>
		public void WriteCopyValueInstruction(IModelField source, IModelField target, BindingTransformation transformationDelegate)
		{
			var sourceField = Expression.Constant(source, typeof(TSourceField));
			var readValue = Expression.Variable(source.FieldType.Type, "readValue");
			var readMethod = _dataModelReaderType.GetMethod(nameof(IDataModelReader<TSourceModel, TSourceField>.ReadField))
					.MakeGenericMethod(source.FieldType.Type);
			var assignReadValueExpression = Expression.Assign(readValue,
				Expression.Call(_sourceReader, readMethod, sourceField));

			var boxedReadValue = Expression.Variable(typeof(object), "boxedReadValue");
			var boxReadValueExpr = Expression.Assign(boxedReadValue, BoxingHelper.BoxIfNeeded(source.FieldType.Type, readValue));

			var boxedConvertedValue = Expression.Variable(typeof(object), "boxedConvertedValue");

			MethodCallExpression callConvertMethodExpr;
			if (transformationDelegate.Method.IsStatic)
			{
				callConvertMethodExpr = Expression.Call(transformationDelegate.Method, _bindingContext, boxedReadValue, boxedConvertedValue);
			}
			else
			{
				callConvertMethodExpr = Expression.Call(Expression.Constant(transformationDelegate.Target), transformationDelegate.Method, _bindingContext, boxedReadValue, boxedConvertedValue);
			}

			var convertedValue = Expression.Variable(target.FieldType.Type, "convertedValue");
			var unboxReadValueExpr = BoxingHelper.UnboxIfNeeded(target.FieldType.Type, boxedConvertedValue);

			var targetField = Expression.Constant(target, typeof(TTargetField));
			var writeMethod = _dataModelWriterType.GetMethod(nameof(IDataModelWriter<TTargetModel, TTargetField>.WriteField))
					.MakeGenericMethod(target.FieldType.Type);
			var writeConvertedValueExpr = Expression.Call(_targetWriter, writeMethod, targetField, unboxReadValueExpr);

			var writeIfConvertedExpr = Expression.IfThen(
				callConvertMethodExpr,
				writeConvertedValueExpr
				);

			_expressionBody.Add(Expression.Block(
				new[] { readValue, boxedReadValue, boxedConvertedValue, convertedValue },
				assignReadValueExpression,
				boxReadValueExpr,
				writeIfConvertedExpr
				));
		}

		public MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField> Build()
		{
			var body = Expression.Block(
				new[] { _bindingContext },
				new Expression[] { Expression.Assign(_bindingContext, Expression.New(typeof(BindingContext))) }
					.Concat(_expressionBody)
				);
			return Expression.Lambda<MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField>>(
				body, _sourceReader, _targetWriter
				).Compile();
		}
	}
}
