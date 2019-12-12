using DataKit.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataKit.Mapping.AspNetCore
{
	public class MappingOptions
	{
		public List<BindingApplicator> Bindings { get; }
			= new List<BindingApplicator>();

		private bool IsBindingOverride(Type type, out (Type FromType, Type ToType) bindingTypes)
		{
			var interfaces = type.GetInterfaces();
			foreach (var @interface in interfaces)
			{
				if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IBindingOverride<,>))
				{
					var genericArgs = @interface.GetGenericArguments();
					bindingTypes = (genericArgs[0], genericArgs[1]);
					return true;
				}
			}

			bindingTypes = default;
			return false;
		}

		public void AddBindingOverridesFromAssembly(Assembly assembly, Func<Type, bool> whiteListFilter = null)
		{
			foreach (var type in assembly.DefinedTypes)
			{
				if (!IsBindingOverride(type, out var bindingTypes))
					continue;
				if (whiteListFilter != null && !whiteListFilter(type))
					continue;

				Bindings.Add(
					Activator.CreateInstance(
						typeof(BindingApplicator<,>).MakeGenericType(bindingTypes.FromType, bindingTypes.ToType),
						new object[] { Activator.CreateInstance(type) }
						) as BindingApplicator
					);
			}
		}

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
