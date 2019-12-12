using DataKit.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.Mapping.AspNetCore
{
	public class MappingOptions
	{
		public List<BindingApplicator> Bindings { get; }
			= new List<BindingApplicator>();

		public void AddBindingOverride<TFrom, TTo>(IBindingOverride<TFrom, TTo> bindingOverride)
			where TFrom : class
			where TTo : class
		{
			Bindings.Add(new BindingApplicator<TFrom, TTo>(bindingOverride));
		}

		public void AddBindingOverride<TOverride, TFrom, TTo>()
			where TOverride : IBindingOverride<TFrom, TTo>, new()
			where TFrom : class
			where TTo : class
		{
			AddBindingOverride(new TOverride());
		}

		public void AddBindingOverride<TFrom, TTo>(Action<TypeBindingBuilder<TFrom,TTo>> binding)
			where TFrom : class
			where TTo : class
		{
			AddBindingOverride(new DelegateBindingOverride<TFrom, TTo>(binding));
		}

		private class DelegateBindingOverride<TFrom, TTo> : IBindingOverride<TFrom, TTo>
			where TFrom : class
			where TTo : class
		{
			private readonly Action<TypeBindingBuilder<TFrom, TTo>> _bindingDelegate;

			public DelegateBindingOverride(Action<TypeBindingBuilder<TFrom, TTo>> bindingDelegate)
			{
				_bindingDelegate = bindingDelegate;
			}

			public void BindFields(TypeBindingBuilder<TFrom, TTo> builder)
			{
				_bindingDelegate(builder);
			}
		}
	}
}
