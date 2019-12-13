using DataKit.Mapping.Binding;
using DataKit.Modelling.TypeModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping.AspNetCore.Mapping
{
	public class BindingProvider
	{
		private readonly Dictionary<(Type, Type), DataModelBinding<PropertyField, PropertyField>> _bindingCache
			= new Dictionary<(Type, Type), DataModelBinding<PropertyField, PropertyField>>();
		private readonly object _lock = new object();

		public BindingProvider(IOptions<MappingOptions> optionsAccessor)
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
				_bindingCache.Add(key, binding);
			}
		}

		public DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetBinding<TFrom, TTo>()
			where TFrom : class
			where TTo : class
		{
			var key = (typeof(TFrom), typeof(TTo));
			if (_bindingCache.TryGetValue(key, out var binding))
				return binding as DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;

			lock (_lock)
			{
				if (_bindingCache.TryGetValue(key, out binding))
					return binding as DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;

				binding = new TypeBindingBuilder<TFrom, TTo>()
					.AutoBind(_bindingCache.Values)
					.BuildBinding();
				_bindingCache.Add(key, binding);
				return binding as DataModelBinding<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField>;
			}
		}
	}
}
