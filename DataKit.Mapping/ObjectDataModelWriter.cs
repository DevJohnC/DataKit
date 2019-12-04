using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class ObjectDataModelWriter<TRoot> : IDataModelWriter<TypeModel<TRoot>, PropertyField>
	{
		private readonly TRoot _root;

		private readonly Stack<StackItem> _objectStack
			= new Stack<StackItem>();

		private readonly IObjectFactory _objectFactory;

		public TypeModel<TRoot> Model { get; }

		public ObjectDataModelWriter(TRoot root, IObjectFactory objectFactory = null)
		{
			_objectFactory = objectFactory ?? DefaultObjectFactory.Instance;
			Model = TypeModel.GetModelOf<TRoot>();
			_objectStack.Push(new StackItem(
				ObjectReaderWriter.ObjectReaderWriter.Get<TRoot>(),
				root,
				TypeModel.GetModelOf<TRoot>()
				));
			_root = root;
		}

		private void EnsureFieldWritable(PropertyField field)
		{
			if (field == null)
				throw new ArgumentNullException(nameof(field));
			if (!field.CanWrite)
				throw new ArgumentException("Field cannot be written.");
		}

		public void LeaveMember()
		{
			var item = _objectStack.Pop();
			if (item.Field == null)
				throw new InvalidOperationException("Leaving member in invalid state.");

			//  commit the built object to it's parent
			var currentItem = _objectStack.Peek();
			var writer = currentItem.Writer.GetWriter(item.Field);
			writer(currentItem.Object, item.Object);
		}

		public void EnterMember(PropertyField field)
		{
			EnsureFieldWritable(field);

			var typeModel = TypeModel.GetModelOf(field.Property.PropertyType);

			if (typeModel.Type.IsPureEnumerable)
			{
				_objectStack.Push(new MutableStackItem(
					ObjectReaderWriter.ObjectReaderWriter.Get(typeModel.Type.Type),
					typeModel,
					field
					));
			}
			else if (!typeModel.Type.IsAnonymousType)
			{
				_objectStack.Push(new StackItem(
					ObjectReaderWriter.ObjectReaderWriter.Get(typeModel.Type.Type),
					_objectFactory.CreateInstance(typeModel.Type.Type),
					typeModel,
					field
					));
			}
			else
				throw new NotSupportedException("Anonymous types not supported.");
		}

		public void WriteField<T>(PropertyField field, T value)
		{
			EnsureFieldWritable(field);

			var writer = _objectStack.Peek().Writer.GetWriter(field);

			writer(_objectStack.Peek().Object, value);
		}

		public void EnterEnumerable()
		{
			var currentItem = _objectStack.Peek();
			if (!currentItem.TypeModel.Type.IsEnumerable)
				throw new InvalidOperationException("Current member is not enumerable.");

			_objectStack.Push(new EnumerableStackItem(
				ObjectReaderWriter.ObjectReaderWriter.Get(currentItem.TypeModel.Type.ElementType),
				TypeModel.GetModelOf(currentItem.TypeModel.Type.ElementType),
				currentItem.Field
				));
		}

		public void LeaveEnumerable()
		{
			var currentItem = _objectStack.Peek();
			if (!(currentItem is EnumerableStackItem enumerableItem))
				throw new InvalidOperationException("Current member is not enumerable.");

			_objectStack.Pop();

			//  commit the built object to it's parent
			currentItem = _objectStack.Peek();
			if (!(currentItem is MutableStackItem mutableItem))
				throw new InvalidOperationException("Current member is not pure enumerable.");

			mutableItem.SetObject(enumerableItem.GetItems());
		}

		public void MoveNext()
		{
			var currentItem = _objectStack.Peek();
			if (!(currentItem is EnumerableStackItem enumerableStackItem))
				throw new InvalidOperationException("Current member is not enumerable.");

			if (currentItem.TypeModel.Type.IsPureEnumerable)
			{
				throw new NotSupportedException("Multi-dimensional enumerables not supported.");
			}
			else if (!currentItem.TypeModel.Type.IsAnonymousType)
			{
				enumerableStackItem.MoveNext(
					_objectFactory.CreateInstance(currentItem.TypeModel.Type.Type)
					);
			}
			else
				throw new NotImplementedException("Anonymous types not supported yet.");
		}

		private class StackItem
		{
			public StackItem(
				ObjectReaderWriter.ObjectReaderWriter writer, object @object,
				TypeModel typeModel, PropertyField field = null)
			{
				Writer = writer ?? throw new ArgumentNullException(nameof(writer));
				Object = @object ?? throw new ArgumentNullException(nameof(@object));
				TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
				Field = field;
			}

			public StackItem(
				ObjectReaderWriter.ObjectReaderWriter writer,
				TypeModel typeModel, PropertyField field = null)
			{
				Writer = writer ?? throw new ArgumentNullException(nameof(writer));
				TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
				Field = field;
			}

			public PropertyField Field { get; }

			public TypeModel TypeModel { get; }

			public ObjectReaderWriter.ObjectReaderWriter Writer { get; }

			public object Object { get; protected set; }
		}

		private class MutableStackItem : StackItem
		{
			public MutableStackItem(
				ObjectReaderWriter.ObjectReaderWriter writer, TypeModel typeModel,
				PropertyField field) : base(writer, typeModel, field)
			{
			}

			public void SetObject(object @object)
			{
				Object = @object;
			}
		}

		private class EnumerableStackItem : StackItem
		{
			private readonly List<object> _items =
				new List<object>();

			public EnumerableStackItem(
				ObjectReaderWriter.ObjectReaderWriter writer, TypeModel typeModel,
				PropertyField field) : base(writer, typeModel, field)
			{
			}

			public List<object> GetItems() => _items;

			public void MoveNext(object newObject)
			{
				_items.Add(newObject);
				Object = newObject;
			}
		}
	}
}
