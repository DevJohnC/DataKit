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

		private readonly ParameterExpression _readNavigator = Expression.Variable(
			typeof(DataModelReaderNavigator<IDataModelReader<TSourceModel, TSourceField>, TSourceModel, TSourceField>),
			"readNavigator");

		private readonly ParameterExpression _writeNavigator = Expression.Variable(
			typeof(DataModelWriterNavigator<IDataModelWriter<TTargetModel, TTargetField>, TTargetModel, TTargetField>),
			"writeNavigator");

		private readonly ParameterExpression _sourceReader = Expression.Parameter(_dataModelReaderType, "sourceReader");

		private readonly ParameterExpression _targetWriter = Expression.Parameter(_dataModelWriterType, "targetWriter");

		public MappingDelegateBlock StartSourceNavigateBlock(params TSourceField[] sourceFields)
		{
			return new MappingDelegateBlock(_sourceReader, _bindingContext, _targetWriter, _readNavigator, _writeNavigator, sourceFields);
		}

		public void EndSourceNavigateBlock(MappingDelegateBlock delegateBlock)
		{
			_expressionBody.Add(
				Expression.IfThen(delegateBlock.EnterPathTest(), delegateBlock.AssignmentsBlock())
				);
		}

		public MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField> Build()
		{
			var readNavigatorCtor = _readNavigator.Type.GetConstructors().First();
			var writeNavigatorCtor = _writeNavigator.Type.GetConstructors().First();
			var body = Expression.Block(
				new[] { _bindingContext, _readNavigator, _writeNavigator },
				new Expression[]
				{
					Expression.Assign(_bindingContext, Expression.New(typeof(BindingContext))),
					Expression.Assign(_readNavigator, Expression.New(readNavigatorCtor, _sourceReader)),
					Expression.Assign(_writeNavigator, Expression.New(writeNavigatorCtor, _targetWriter))
				}.Concat(_expressionBody).Concat(new Expression[] {
					Expression.Call(_writeNavigator, _writeNavigator.Type.GetMethod(
						nameof(DataModelWriterNavigator<IDataModelWriter<TTargetModel, TTargetField>, TTargetModel, TTargetField>.SeekToTop)))
				})
				);
			return Expression.Lambda<MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField>>(
				body, _sourceReader, _targetWriter
				).Compile();
		}

		public class MappingDelegateBlock
		{
			private readonly ParameterExpression _sourceReader;
			private readonly ParameterExpression _bindingContext;
			private readonly ParameterExpression _targetWriter;
			private readonly ParameterExpression _readNavigator;
			private readonly ParameterExpression _writeNavigator;
			private readonly TSourceField[] _sourceFields;
			private readonly List<Expression> _expressionBody = new List<Expression>();

			public MappingDelegateBlock(ParameterExpression sourceReader, ParameterExpression bindingContext, ParameterExpression targetWriter,
				ParameterExpression readNavigator, ParameterExpression writeNavigator, TSourceField[] sourceFields)
			{
				_sourceReader = sourceReader;
				_bindingContext = bindingContext;
				_targetWriter = targetWriter;
				_readNavigator = readNavigator;
				_writeNavigator = writeNavigator;
				_sourceFields = sourceFields;
			}

			public void WriteNavigateToReaderPositionInstruction(TTargetField[] targetFields)
			{
				var seekMathod = _writeNavigator.Type.GetMethod(
					nameof(DataModelWriterNavigator<IDataModelWriter<TTargetModel, TTargetField>, TTargetModel, TTargetField>.SeekToPosition)
					);
				_expressionBody.Add(Expression.Call(
					_writeNavigator, seekMathod, Expression.Constant(targetFields)
					));
			}

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

			public Expression EnterPathTest()
			{
				var trySeekMethod = _readNavigator.Type.GetMethod(
					nameof(DataModelReaderNavigator<IDataModelReader<TSourceModel, TSourceField>,TSourceModel,TSourceField>.TrySeekToPosition)
					);
				return Expression.IsTrue(Expression.Call(
					_readNavigator, trySeekMethod, Expression.Constant(_sourceFields)
					));
			}

			public Expression AssignmentsBlock()
			{
				return Expression.Block(_expressionBody);
			}
		}
	}
}
