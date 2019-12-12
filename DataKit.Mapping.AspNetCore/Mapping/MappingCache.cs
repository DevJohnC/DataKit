using DataKit.Mapping.Binding;
using DataKit.Modelling;
using DataKit.Modelling.TypeModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping.AspNetCore.Mapping
{
	public class MappingCache
	{
		private readonly CachedCollection<(Type, Type), DataKit.Mapping.Mapping> _mappingCache
			= new CachedCollection<(Type, Type), DataKit.Mapping.Mapping>();
		private readonly Dictionary<(Type, Type), DataModelBinding<PropertyField, PropertyField>> _bindings
			= new Dictionary<(Type, Type), DataModelBinding<PropertyField, PropertyField>>();
		private readonly object _mappingCreationLock = new object();

		public MappingCache(IOptions<MappingOptions> optionsAccessor)
		{
			ApplyConfiguredBindings(optionsAccessor.Value.Bindings);
		}

		private void ApplyConfiguredBindings(IEnumerable<BindingApplicator> bindings)
		{
			foreach (var bindingApp in bindings)
			{
				var binding = bindingApp.CreateBinding();
				var sourceModel = binding.SourceModel as TypeModel;
				var targetModel = binding.TargetModel as TypeModel;
				var key = (sourceModel.Type.Type, targetModel.Type.Type);
				_bindings.Add(key, binding);
			}
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
			lock (_mappingCreationLock)
			{
				var cacheKey = (typeof(TFrom), typeof(TTo));
				if (!_bindings.TryGetValue(cacheKey, out var binding))
				{
					binding = new TypeBindingBuilder<TFrom, TTo>()
						.AutoBind(_bindings.Values)
						.BuildBinding();

					_bindings.Add(cacheKey, binding);
				}

				return ((DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>)binding)
					.BuildMapping();
			}
		}
	}
}
