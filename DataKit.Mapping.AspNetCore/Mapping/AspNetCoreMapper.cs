using DataKit.Modelling.TypeModels;
using System;
using System.Collections.Generic;

namespace DataKit.Mapping.AspNetCore.Mapping
{
	public class AspNetCoreMapper : ObjectMapperBase
	{
		private readonly MappingCache _mappings;

		public AspNetCoreMapper(
			IObjectFactory objectFactory,
			MappingCache mappings
			) : base(objectFactory)
		{
			_mappings = mappings;
		}

		protected override Mapping<TypeModel<TFrom>, PropertyField, TypeModel<TTo>, PropertyField> GetMapping<TFrom, TTo>()
			=> _mappings.GetMapping<TFrom, TTo>();
	}
}
