using DataKit.Modelling.TypeModels;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class ObjectDataModelReader<TRoot> : IDataModelReader<TypeModel<TRoot>, PropertyField>
	{
		private readonly TRoot _root;

		private readonly Stack<StackItem> _objectStack
			= new Stack<StackItem>();

		public TypeModel<TRoot> Model { get; }

		public ObjectDataModelReader(TRoot root)
		{
			Model = TypeModel.GetModelOf<TRoot>();
			_objectStack.Push(new ObjectStackItem(
				ObjectReaderWriter.ObjectReaderWriter.Get<TRoot>(),
				root,
				TypeModel.GetModelOf<TRoot>()
				));
			_root = root;
		}

		protected void PushOntoMemberStack(StackItem stackItem)
			=> _objectStack.Push(stackItem);

		protected StackItem PopFromMemberStack()
			=> _objectStack.Pop();

		private void EnsureFieldReadable(PropertyField field)
		{
			if (field == null)
				throw new ArgumentNullException(nameof(field));
			if (!field.CanRead)
				throw new ArgumentException("Field cannot be read.");
		}

		public T ReadField<T>(PropertyField field)
		{
			EnsureFieldReadable(field);

			var reader = _objectStack.Peek()
				.Reader
				.GetReader(field);

			return (T)reader(_objectStack.Peek().Object);
		}

		public void EnterMember(PropertyField field)
		{
			EnsureFieldReadable(field);

			var reader = _objectStack.Peek()
				.Reader
				.GetReader(field);

			var instance = reader(_objectStack.Peek().Object);
			if (instance == null)
				throw new InvalidOperationException("Cannot enter a member with a null value.");

			_objectStack.Push(new ObjectStackItem(
				ObjectReaderWriter.ObjectReaderWriter.Get(field.Property.PropertyType),
				instance,
				TypeModel.GetModelOf(field.Property.PropertyType)
				));
		}

		public void LeaveMember()
		{
			_objectStack.Pop();
		}

		public bool MoveNext()
		{
			var currentItem = _objectStack.Peek();
			if (!(currentItem is EnumeratorStackItem enumerator))
				throw new InvalidOperationException("Current member is not enumerable.");

			return enumerator.MoveNext();
		}

		public void EnterEnumerable()
		{
			var currentItem = _objectStack.Peek();
			if (!currentItem.TypeModel.Type.IsEnumerable)
				throw new InvalidOperationException("Current member is not enumerable.");

			var enumerator = ((IEnumerable)currentItem.Object).GetEnumerator();

			_objectStack.Push(new EnumeratorStackItem(
				ObjectReaderWriter.ObjectReaderWriter.Get(currentItem.TypeModel.Type.ElementType),
				TypeModel.GetModelOf(currentItem.TypeModel.Type.ElementType),
				enumerator
				));
		}

		public void LeaveEnumerable()
		{
			var currentItem = _objectStack.Peek();
			if (!(currentItem is EnumeratorStackItem enumerator))
				throw new InvalidOperationException("Current member is not enumerable.");

			_objectStack.Pop();
			enumerator.Dispose();
		}

		public abstract class StackItem
		{
			public abstract TypeModel TypeModel { get; }

			public abstract ObjectReaderWriter.ObjectReaderWriter Reader { get; }

			public abstract object Object { get; set; }
		}

		private class ObjectStackItem : StackItem
		{
			public ObjectStackItem(
				ObjectReaderWriter.ObjectReaderWriter reader, object @object,
				TypeModel typeModel)
			{
				Reader = reader ?? throw new ArgumentNullException(nameof(reader));
				Object = @object;
				TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
			}

			public ObjectStackItem(ObjectReaderWriter.ObjectReaderWriter reader, TypeModel typeModel)
			{
				Reader = reader ?? throw new ArgumentNullException(nameof(reader));
				TypeModel = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
			}

			public override TypeModel TypeModel { get; }

			public override ObjectReaderWriter.ObjectReaderWriter Reader { get; }

			public override object Object { get; set; }
		}

		private class EnumeratorStackItem : ObjectStackItem, IDisposable
		{
			private readonly IEnumerator _enumerator;

			public EnumeratorStackItem(ObjectReaderWriter.ObjectReaderWriter reader, TypeModel typeModel, IEnumerator enumerator) :
				base(reader, typeModel)
			{
				_enumerator = enumerator;
			}

			public bool MoveNext()
			{
				if (!_enumerator.MoveNext())
				{
					Object = null;
					return false;
				}

				Object = _enumerator.Current;
				return true;
			}

			public void Dispose()
			{
				if (_enumerator is IDisposable disposable)
					disposable.Dispose();
			}
		}
	}
}
