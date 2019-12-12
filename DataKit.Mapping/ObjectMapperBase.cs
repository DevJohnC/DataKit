using DataKit.Modelling.TypeModels;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public abstract class ObjectMapperBase : IObjectMapper
	{
		protected readonly IObjectFactory objectFactory;

		protected ObjectMapperBase(IObjectFactory objectFactory)
		{
			this.objectFactory = objectFactory;
		}

		protected abstract Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetMapping<TFrom, TTo>()
			where TFrom : class
			where TTo : class;

		public void Inject<TFrom, TTo>(TFrom from, TTo to)
			where TFrom : class
			where TTo : class
		{
			InjectSingle(from, to, GetMapping<TFrom, TTo>());
		}

		protected virtual void InjectSingle<TFrom, TTo>(TFrom from, TTo to,
			Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> mapping)
			where TFrom : class
			where TTo : class
		{
			var reader = new ObjectDataModelReader<TFrom>(from);
			var writer = new ObjectDataModelWriter<TTo>(to, objectFactory);
			mapping.Run(reader, writer);
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
			var to = objectFactory.CreateInstance<TTo>();
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
				var to = objectFactory.CreateInstance<TTo>();
				InjectSingle(obj, to, mapping);
				yield return to;
			}
		}
	}
}
