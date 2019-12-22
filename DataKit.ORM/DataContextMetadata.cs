using DataKit.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IQueryProvider = DataKit.SQL.Providers.IQueryProvider;

namespace DataKit.ORM
{
	internal abstract class DataContextMetadata
	{
		private static MethodInfo _createMetadataOpenGenericMethod = typeof(DataContextMetadata)
			.GetMethod(nameof(CreateMetadataImpl), BindingFlags.NonPublic | BindingFlags.Static);

		public static DataContextMetadata CreateMetadata(Type dataContextType)
		{
			return _createMetadataOpenGenericMethod
				.MakeGenericMethod(dataContextType)
				.Invoke(null, new object[0]) as DataContextMetadata;
		}

		private static DataContextMetadata<T> CreateMetadataImpl<T>()
			where T : DataContext
		{
			var sqlDataSets = new List<SqlDataSetPropertyMetadata<T>>();

			foreach (var property in typeof(T).GetDataSetProperties())
			{
				if (property.PropertyType.IsSqlDataSetWithEntityType() ||
					property.PropertyType.IsSqlDataSetWithEntityAndBusinessTypes())
				{
					sqlDataSets.Add(
						SqlDataSetPropertyMetadata<T>.Create(property)
						);
				}
			}

			return new DataContextMetadata<T>(sqlDataSets);
		}
	}

	internal class DataContextMetadata<TDataContext> : DataContextMetadata
		where TDataContext : DataContext
	{
		public IReadOnlyList<SqlDataSetPropertyMetadata<TDataContext>> SqlDataSets { get; }

		public DataContextMetadata(IReadOnlyList<SqlDataSetPropertyMetadata<TDataContext>> sqlDataSets)
		{
			SqlDataSets = sqlDataSets;
		}
	}

	internal class SqlDataSetPropertyMetadata<TDataContext>
		where TDataContext : DataContext
	{
		public Action<TDataContext, IQueryProvider, DataSchema, DataContextCreationOptions> _createAndAssignDataSet;

		public SqlDataSetPropertyMetadata(
			Action<TDataContext, IQueryProvider, DataSchema, DataContextCreationOptions> createAndAssignDataSet
			)
		{
			_createAndAssignDataSet = createAndAssignDataSet ?? throw new ArgumentNullException(nameof(createAndAssignDataSet));
		}

		public void CreateAndAssignDataSet(TDataContext dataContext, IQueryProvider queryProvider,
			DataSchema dataSchema, DataContextCreationOptions dataContextCreationOptions)
			=> _createAndAssignDataSet(dataContext, queryProvider,
				dataSchema, dataContextCreationOptions);

		public static SqlDataSetPropertyMetadata<TDataContext> Create(PropertyInfo fromProperty)
		{
			return new SqlDataSetPropertyMetadata<TDataContext>(
				CreateAssignmentMethod(fromProperty));
		}

		private static Action<TDataContext, IQueryProvider, DataSchema, DataContextCreationOptions> CreateAssignmentMethod(
			PropertyInfo fromProperty)
		{
			var dataSetGenericTypeArguments = fromProperty.PropertyType.GetGenericArguments();
			var entityType = dataSetGenericTypeArguments.Last();
			var dataSetType = fromProperty.PropertyType.GetGenericTypeDefinition();

			var dataContext = Expression.Parameter(typeof(TDataContext), "dataContext");
			var dataProvider = Expression.Parameter(typeof(IQueryProvider), "queryProvider");
			var schema = Expression.Parameter(typeof(DataSchema), "schema");
			var creationOptions = Expression.Parameter(typeof(DataContextCreationOptions), "creationOptions");

			var createDataSetMethod = typeof(DataContextCreationOptions)
				.GetMethods()
				.First(q => q.ReturnType.IsGenericType && q.ReturnType.GetGenericTypeDefinition() == dataSetType)
				.MakeGenericMethod(dataSetGenericTypeArguments);

			var assignmentExpression = Expression.Assign(
				Expression.Property(dataContext, fromProperty),
				Expression.Call(creationOptions, createDataSetMethod, schema, dataProvider)
				); ;

			return Expression.Lambda<Action<TDataContext, IQueryProvider, DataSchema, DataContextCreationOptions>>(
				assignmentExpression,
				dataContext,
				dataProvider,
				schema,
				creationOptions).Compile();
		}
	}
}
