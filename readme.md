# DataKit

An collection of libraries for operating on data in .NET standard 2.1/dotnet core 3.0 and up.

`DataKit` features libraries for modelling data structured, binding and mapping, and SQL ORM capabilities.

This is the spiritual successor to my work on SilkStack.Data, more of which will be migrating into DataKit.

## Highlights

`DataKit` contains an ORM that fully integrates with the mapping and binding library.
This means that your declared type bindings can be used to control how data is queried from a data store.

# Libraries

## DataKit.Mapping

APIs for binding and mapping between data types.

## DataKit.ORM

APIs for querying SQL data stores. Designed to be type-safe, extensible and capable of batching multiple queries into single round-trips to the database server.

[Learn more about DataKit.ORM](DataKit.ORM/readme.md)

# License

All of `DataKit` is licensed under the MIT license.