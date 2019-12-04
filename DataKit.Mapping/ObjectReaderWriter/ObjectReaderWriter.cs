using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataKit.Mapping.ObjectReaderWriter
{
	public abstract class ObjectReaderWriter
	{
		public delegate object ReaderDelegate(object instance);

		public delegate void WriterDelegate(object instance, object value);

		private static readonly CachedCollection<Type, Type, ObjectReaderWriter> _cache =
			new CachedCollection<Type, Type, ObjectReaderWriter>(Create);

		public static ObjectReaderWriter<T> Get<T>()
			=> Get(typeof(T)) as ObjectReaderWriter<T>;

		public static ObjectReaderWriter Get(Type type)
			=> _cache.GetOrCreate(type, type);

		private static ObjectReaderWriter Create(Type type)
			=> Activator.CreateInstance(
				typeof(ObjectReaderWriter<>).MakeGenericType(type)
				) as ObjectReaderWriter;

		public abstract ReaderDelegate GetReader(PropertyField propertyField);

		public abstract WriterDelegate GetWriter(PropertyField propertyField);
	}

	public class ObjectReaderWriter<T> : ObjectReaderWriter
	{
		private static readonly CachedCollection<PropertyField, PropertyField, ReaderDelegate>
			_propertyReaders = new CachedCollection<PropertyField, PropertyField, ReaderDelegate>(CreateReader);

		private static readonly CachedCollection<PropertyField, PropertyField, WriterDelegate>
			_propertyWriters = new CachedCollection<PropertyField, PropertyField, WriterDelegate>(CreateWriter);

		private static ReaderDelegate CreateReader(PropertyField propertyField)
		{
			var instanceExpression = Expression.Parameter(typeof(object));

			return Expression.Lambda<ReaderDelegate>(
				BoxingHelper.BoxIfNeeded(propertyField.Property.PropertyType, Expression.Property(
					Expression.Convert(instanceExpression, typeof(T)), propertyField.Property
					)),
				instanceExpression
				).Compile();
		}

		private static WriterDelegate CreateWriter(PropertyField propertyField)
		{
			if (propertyField.FieldType.IsPureEnumerable)
			{
				var instanceExpr = Expression.Parameter(typeof(object));
				var valueExpr = Expression.Parameter(typeof(object));
				var convertedValueExpr = Expression.Convert(valueExpr, typeof(IEnumerable<object>));

				Expression convertExpression;
				if (propertyField.FieldType.Type.IsArray)
				{
					var method = typeof(ObjectReaderWriter<T>).GetMethod(nameof(ConvertToArray),
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
						.MakeGenericMethod(propertyField.FieldType.ElementType);
					convertExpression = Expression.Call(method, convertedValueExpr);
				}
				else
				{
					var method = typeof(ObjectReaderWriter<T>).GetMethod(nameof(ConvertToList),
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
						.MakeGenericMethod(propertyField.FieldType.ElementType);
					convertExpression = Expression.Call(method, convertedValueExpr);
				}

				return Expression.Lambda<WriterDelegate>(
					Expression.Assign(
						Expression.Property(Expression.Convert(instanceExpr, typeof(T)), propertyField.Property),
						convertExpression
						), instanceExpr, valueExpr
					).Compile();
			}
			else
			{
				var instanceExpr = Expression.Parameter(typeof(object));
				var valueExpr = Expression.Parameter(typeof(object));

				return Expression.Lambda<WriterDelegate>(
					Expression.Assign(
						Expression.Property(Expression.Convert(instanceExpr, typeof(T)), propertyField.Property),
						BoxingHelper.UnboxIfNeeded(propertyField.Property.PropertyType, valueExpr)
						), instanceExpr, valueExpr
					).Compile();
			}
		}

		private static List<TData> ConvertToList<TData>(IEnumerable<object> input)
			=> input.OfType<TData>().ToList();

		private static TData[] ConvertToArray<TData>(IEnumerable<object> input)
			=> input.OfType<TData>().ToArray();

		public override ReaderDelegate GetReader(PropertyField propertyField)
			=> _propertyReaders.GetOrCreate(propertyField, propertyField);

		public override WriterDelegate GetWriter(PropertyField propertyField)
			=> _propertyWriters.GetOrCreate(propertyField, propertyField);
	}
}
