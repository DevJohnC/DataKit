using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using System;

namespace DataKit.Mapping.AspNetCore.Mapping
{
	public class MappingProvider
	{
		private readonly CachedCollection<(Type, Type), DataKit.Mapping.Mapping> _mappingCache
			= new CachedCollection<(Type, Type), DataKit.Mapping.Mapping>();
		private readonly BindingProvider _bindingProvider;

		public MappingProvider(BindingProvider bindingProvider)
		{
			_bindingProvider = bindingProvider;
		}

		public Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetMapping<TFrom, TTo>()
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
			return _bindingProvider.GetBinding<TFrom, TTo>()
				.BuildMapping();
		}
	}
}
