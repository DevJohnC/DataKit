using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class DefaultObjectMapper : ObjectMapperBase, DefaultObjectMapper.IBindingFactory
	{
		private readonly CachedCollection<(Type, Type), Mapping> _mappingCache
			= new CachedCollection<(Type, Type), Mapping>();
		private readonly IBindingFactory _bindingFactory;

		public DefaultObjectMapper(IObjectFactory objectFactory = null,
			IBindingFactory bindingFactory = null) :
			base(objectFactory ?? DefaultObjectFactory.Instance)
		{
			_bindingFactory = bindingFactory ?? this;
		}

		protected override Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetMapping<TFrom, TTo>()
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
