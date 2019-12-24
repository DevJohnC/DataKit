# DataKit.ORM

## What is DataKit.ORM

DataKit.ORM is a cross-platform ORM library for .NET.

### Supported Platforms

- netstandard 2.0 (compatible with .NET framework 4.6.2, 4.7.2 recommended and .netcore 2.0+)
- netstandard 2.1 (compatible with .netcore 3.0+)

### Features

- **CRUD operations** Insert, update, delete or select entity instances, projections or expressions.
- **Custom mappings** Override and customize how projections are bound to your entity types with the full capability of DataKit.Mapping.
- **Batch execution** Minimize round-trips to your data store by running queries in a single query.
- **Database agnostic API** Run your queries against any supported data store without code changes, includes support for SQLite3, Postgresql, MySQL/MariaDB and Microsoft SQL Server.
- **Custom database functions** Declare your database functions in C# and translate them to SQL using the DataKit.SQL expressions API to remain database agnostic.
- **Cancellable APIs**
- **Full async and non-async API surface**

### Goals

DataKit.ORM is a library designed to have a type-safe API for querying various types of data stores for data without needlessly hiding that data stores are being queried.

Currently only SQL is supported, and simple entity management and queries at that, but I hope to introduce support for more expressive query APIs for SQL
catering to more usage scenarios like report generation, installing functions/procedures etc.

Support for graph databases and nosql key-value stores is hopefully going to be added also.

## License Information

DataKit.ORM is licensed under the MIT license and is Copyright (c) 2019 John Carruthers.

```
The MIT License (MIT)
Copyright (c) 2019 John Carruthers

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

## Using DataKit.ORM

### DataContext

Similarly to EntityFramework, DataKit.ORM makes use of a DataContext type that defines your schema and provides access to the query APIs.

It should be noted you can build a schema yourself and create DataSet instances manually with ease but the DataContext makes it the easiest.

To create a DataContext just create a class that extends `DataContext` and declare your schema members:

```
public class MyEntity
{
	public int Id { get; protected set; }
	public string Data { get; set; }
}

public class MyDataContext : DataContext
{
	public SqlDataSet<MyEntity> MyEntities { get; private set; }
}

using (var sqlProvider = Sqlite3DataProvider.InMemory())
{
	//  ...snip table creation...
	var dataContext = DataContext.Create<MyDataContext>(sqlProvider);
	var myRows = await dataContext.MyEntities.Select().ToListAsync();
}
```

### Queries

Queries are built using the `DataSet` APIs attached to your DataContext.

SQL query support is provided by `SqlDataSet<TEntity>` where `TEntity` is your entity type being stored in the SQL database.

**SELECT:**

```
var myEntity = await dataContext.MyEntities.Select()
	.AndWhere(q => q.Id == 1).ToSingleAsync();

var myProjection = await dataContext.MyEntities.Select<ProjectionType>()
	.AndWhere(q => q.Id == 1).ToSingleAsync();

var myData = await dataContext.MyEntities.Select(q => q.Data)
	.AndWhere(q => q.Id == 1).ToSingleAsync();

var (myId, myExpression) = await dataContext.MyEntities
	.Select(q => q.Id)
	.Select(q => q.Id + 1)
	.AndWhere(q => q.Id == 1)
	.ToSingleAsync();
```

**INSERT:**

```
await dataContext.MyEntities.Insert(new MyEntity { Data = "Hello World" }).ExecuteAsync();

await dataContext.MyEntities.Insert(new { Data = "Hello World" }).ExecuteAsync();

await dataContext.MyEntities.Insert().Set(q => q.Data, "Hello World").ExecuteAsync();
```

**UPDATE:**

```
await dataContext.MyEntities.Update(entityInstanceWithIdSet).ExecuteAsync();

await dataContext.MyEntities.Update(new { Data = "Hello World" })
	.AndWhere(q => q.Id == 1).ExecuteAsync();

await dataContext.MyEntities.Update()
	.Set(q => q.Data, "Hello World")
	.AndWhere(q => q.Id == 1)
	.ExecuteAsync();
```

**DELETE:**

```
await dataContext.MyEntities.Delete(entityInstanceWithIdSet).ExecuteAsync();

await dataContext.MyEntities.Delete()
	.AndWhere(q => q.Id == 1).ExecuteAsync();
```

**BATCH:**

```
await Batch.Create()
	.Insert(dataContext.MyEntities.Insert(newEntityOne))
	.Insert(dataContext.MyEntities.Insert(newEntityTwo))
	.Update(dataContext.MyEntities.Update().Set(q => q.Value, q => q.Value + 1))
	.Inject(dataContext.MyEntities.Select().AndWhere(q => q.Id == 2), retrievedEntity)
	.Delete(dataContext.MyEntities.Delete().AndWhere(q => q.Id == 1))
	.List(dataContext.MyEntities.Select(q => q.Value), out var fullValueSetResult)
	.Single(dataContext.MyEntities.Select(q => q.Id), out var singleIdResult)
	.Execute();
```

### Mappings

Both the `Insert` and `Update` APIs take a binding argument that controls how your projection type is bound to your entity type.
Similarly, the `Select` API takes an argument that controls how your entity type is bound onto your projection type.

Such bindings can even involve a data transformation step to transform your stored data into something more suitable for your application to use.

```
var toStoreBinding = new TypeBindingBuilder<MyProjection, MyEntity>()
	.Bind(projection => projection.Slug, entity => entity.NormalizedSlug, value => value.ToLowerInvariant())
	.BuildBinding();

await dataContext.MyEntities.Insert(new MyProjection { Slug = "NorMalIzE Me" }, toStoreBinding).ExecuteAsync();

var fromStoreBinding = new TypeBindingBuilder<MyEntity, MyProjection>()
	.Bind(entity => entity.Title, projection => projection.Title, value => value.CapializeWords())
	.BuildBinding();

var row = await dataContext.MyEntityes.Select<MyProjection>(fromStoreBinding).ToSingleAsync();
```

When using DataKit.ORM.AspNetCore to provide ORM functionality to your AspNetCore project the binding overrides you configure to use with mapping will also be used, automatically, when executing SQL queries;
allowing you to maintain custom binding rules in one place.

### Custom Database Functions

#### SQL functions

DataKit.ORM allows you to define APIs that can be utilized in your query expressions and swapped out for a QueryExpression when executed against the database.

SQL functions are defined as extension methods on the `SqlServerFunctions` type and typically don't have an implementation in C#.

By convention, for ease of use these extensions are normally defined in the namespace `DataKit.ORM`.

```
namespace DataKit.ORM
{
	public static class SqlFunctionExtensions
	{
		public static bool IsSuperUser(this SqlServerFunctions s, Account account)
			=> false; //  no real impl.
	}
}
```

Now create a converter:

```
private class ServerFunctionConverter : ISqlMethodCallConverter
{
	public bool TryConvertMethod<TEntity>(MethodCallExpression methodCallExpression, ExpressionConversionVisitor<TEntity> expressionConverter, out LinqQueryExpression<TEntity> convertedExpression) where TEntity : class
	{
		if (methodCallExpression.Method.Name != nameof(SqlFunctionExtensions.IsSuperUser))
		{
			convertedExpression = default;
			return false;
		}

		var typedConverter = expressionConverter as ExpressionConversionVisitor<Account>;
		convertedExpression = Convert(typedConverter, q => (q.Flags & AccountFlags.SuperUser) == AccountFlags.SuperUser) as LinqQueryExpression<TEntity>;
		return true;
	}

	private LinqQueryExpression<Flat_Entity> Convert(ExpressionConversionVisitor<Account> converter, Expression<Func<Account, bool>> expr)
	{
		var conditionConverter = new ConditionConverter<Account>(
			converter.DataModel,
			converter.TableIdentifier
			);
		var converted = conditionConverter.ConvertClause(expr);
		return new LinqQueryExpression<Account>(
			converted.QueryExpression, converted.Joins
			);
	}
}
```

Finally, add a static method in your DataContext type to configure the schema:

```
public static void ConfigureSchema(DataSchemaBuilder builder)
{
	builder.AddSqlMethodCallConverter(new ServerFunctionConverter());
}
```

Now you can write SQL statements using the newly defined method:

```
var superUsers = dataContext.Users.Select()
	.AndWhere(q => dataContext.SqlServerFunctions.IsSuperUser(q))
	.ToList();
```