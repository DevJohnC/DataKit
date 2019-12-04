using DataKit.Modelling;
using DataKit.ORM.Schema;
using DataKit.ORM.Sql;
using Silk.Data.SQL.Providers;
using System;
using System.Threading.Tasks;

namespace DataKit.ORM
{
	public abstract class DataContext : IDisposable
	{
		private static readonly CachedCollection<Type, Type, DataSchema> _schemas
			= new CachedCollection<Type, Type, DataSchema>(DataContextSchemaFactory.CreateSchemaFromDataContextType);

		private static readonly CachedCollection<Type, Type, DataContextMetadata> _metadata
			= new CachedCollection<Type, Type, DataContextMetadata>(DataContextMetadata.CreateMetadata);

		public static T Create<T>(IDataProvider dataProvider, DataContextCreationOptions dataContextCreationOptions = null)
			where T : DataContext, new()
		{
			if (dataProvider == null)
				throw new ArgumentNullException(nameof(dataProvider));

			var dataProviderProxy = new TransactingDataProviderProxy(dataProvider);

			if (dataContextCreationOptions == null)
				dataContextCreationOptions = DataContextCreationOptions.Default;

			var dataContextType = typeof(T);
			var schema = _schemas.GetOrCreate(dataContextType, dataContextType);

			return BindDataSets(new T
			{
				DataSchema = schema,
				_dataProvider = dataProviderProxy
			}, dataContextCreationOptions, schema, dataProviderProxy);
		}

		private static T BindDataSets<T>(
			T dataContext, DataContextCreationOptions dataContextCreationOptions,
			DataSchema schema, IQueryProvider queryProvider
			) where T : DataContext
		{
			var metadata = _metadata.GetOrCreate(typeof(T), typeof(T)) as DataContextMetadata<T> ??
				throw new Exception("Metadata cache corrupted.");

			foreach (var sqlProperty in metadata.SqlDataSets)
			{
				sqlProperty.CreateAndAssignDataSet(
					dataContext, queryProvider, schema,
					dataContextCreationOptions);
			}

			return dataContext;
		}

		public void Dispose()
		{
			_transaction?.Dispose();
			//  do NOT dispose the data provider here
			//  the underlying data provider will be disposed and they are designed
			//  to be disposed higher up in the application layers/using a IoC container
			//_dataProvider.Dispose();
		}

		public Transaction CreateTransaction()
		{
			if (_transaction != null)
				return _transaction;

			var dbTransaction = _dataProvider.CreateTransaction();

			_transaction = new Transaction(dbTransaction);
			return _transaction;
		}

		public async Task<Transaction> CreateTransactionAsync()
		{
			if (_transaction != null)
				return _transaction;

			var dbTransaction = await _dataProvider.CreateTransactionAsync();

			_transaction = new Transaction(dbTransaction);
			return _transaction;
		}

		public DataSchema DataSchema { get; private set; }

		private TransactingDataProviderProxy _dataProvider;
		private Transaction _transaction;

		public IQueryProvider SqlQueryProvider => _dataProvider;

		public SqlServerFunctions SqlServerFunctions { get; }
	}
}
