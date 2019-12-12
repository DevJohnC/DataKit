using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class DefaultObjectMapper : IObjectMapper, DefaultObjectMapper.IBindingFactory
	{
		private readonly IObjectFactory _objectFactory;
		private readonly CachedCollection<(Type, Type), Mapping> _mappingCache
			= new CachedCollection<(Type, Type), Mapping>();
		private readonly IBindingFactory _bindingFactory;

		public DefaultObjectMapper(IObjectFactory objectFactory = null,
			IBindingFactory bindingFactory = null)
		{
			_objectFactory = objectFactory ?? DefaultObjectFactory.Instance;
			_bindingFactory = bindingFactory ?? this;
		}

		private Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetMapping<TFrom, TTo>()
			where TFrom : class
			where TTo : class
		{
			var key = (typeof(TFrom), typeof(TTo));
			if (_mappingCache.TryGetValue(key, out var mapping))
				return mapping as Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;

			return _mappingCache.CreateIfNeeded(key, MappingFactory<TFrom, TTo>)
				as Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;
		}

		private Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> MappingFactory<TFrom, TTo>()
			where TFrom : class
			where TTo : class
		{
			return _bindingFactory.GetBindingForMapping<TFrom, TTo>()
				.BuildMapping();
		}

		private void InjectSingle<TFrom, TTo>(TFrom from, TTo to,
			Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> mapping)
			where TFrom : class
			where TTo : class
		{
			var reader = new ObjectDataModelReader<TFrom>(from);
			var writer = new ObjectDataModelWriter<TTo>(to, _objectFactory);
			mapping.Run(reader, writer);
		}

		public void Inject<TFrom, TTo>(TFrom from, TTo to)
			where TFrom : class
			where TTo : class
		{
			InjectSingle(from, to, GetMapping<TFrom, TTo>());
		}

		public void InjectAll<TFrom, TTo>(IEnumerable<TFrom> from, IEnumerable<TTo> to)
			where TFrom : class
			where TTo : class
		{
			var mapping = GetMapping<TFrom, TTo>();
			using (var fromEnumerator = from.GetEnumerator())
			using (var toEnumerator = to.GetEnumerator())
			{
				while (fromEnumerator.MoveNext() && toEnumerator.MoveNext())
				{
					InjectSingle(fromEnumerator.Current, toEnumerator.Current, mapping);
				}
			}
		}

		public TTo Map<TFrom, TTo>(TFrom from)
			where TFrom : class
			where TTo : class
		{
			var to = _objectFactory.CreateInstance<TTo>();
			InjectSingle(from, to, GetMapping<TFrom, TTo>());
			return to;
		}

		public IEnumerable<TTo> MapAll<TFrom, TTo>(IEnumerable<TFrom> from)
			where TFrom : class
			where TTo : class
		{
			var mapping = GetMapping<TFrom, TTo>();
			foreach (var obj in from)
			{
				var to = _objectFactory.CreateInstance<TTo>();
				InjectSingle(obj, to, mapping);
				yield return to;
			}
		}

		DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> IBindingFactory.GetBindingForMapping<TFrom, TTo>()
			=> new TypeBindingBuilder<TFrom, TTo>()
				.AutoBind()
				.BuildBinding();

		public interface IBindingFactory
		{
			DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetBindingForMapping<TFrom, TTo>()
				where TFrom : class
				where TTo : class;
		}
	}
}
