using System.Collections.Generic;

namespace DataKit.Mapping
{
	public interface IObjectMapper
	{
		/// <summary>
		/// Map from <see cref="from"/> to a new instance of <see cref="TTo"/>.
		/// </summary>
		TTo Map<TFrom, TTo>(TFrom from)
			where TFrom : class
			where TTo : class;

		/// <summary>
		/// Map from enumerable <see cref="from"/> one at a time.
		/// </summary>
		/// <typeparam name="TTo"></typeparam>
		/// <param name="from"></param>
		/// <returns></returns>
		IEnumerable<TTo> MapAll<TFrom, TTo>(IEnumerable<TFrom> from)
			where TFrom : class
			where TTo : class;

		/// <summary>
		/// Inject values from <see cref="from"/> into <see cref="to"/>.
		/// </summary>
		void Inject<TFrom, TTo>(TFrom from, TTo to)
			where TFrom : class
			where TTo : class;

		/// <summary>
		/// Inject values from all <see cref="from"/> into <see cref="to"/> one at a time.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		void InjectAll<TFrom, TTo>(IEnumerable<TFrom> from, IEnumerable<TTo> to)
			where TFrom : class
			where TTo : class;
	}
}
