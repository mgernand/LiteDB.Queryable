# LiteDB.Queryable

An IQueryable wrapper implementation for LiteDB with additional async extensions.

This library allows the use of LINQ extensions methods for querying LiteDB.

The ```LiteQueryable<T>``` implementation is a warpper around a ```ILiteCollection<T>```
or a ```ILiteCollectionAsync<>```. The LINQ extenions call are delegated to the 
equivalent methods of the LiteDB API.

## Usage

Add the NuGet package to your project: ```LiteDB.Queryable```

You can then aquire an ```IQueryable<T>``` instance using one of the available ```AsQueryable```
extension methods for ```ILiteCollection<T>``` or ```ILiteCollectionAsync<T>```. The async 
variant is provided by the [litedb-async](https://github.com/mlockett42/litedb-async) project.

```C#
LiteDatabase database = new LiteDatabase("test.db");
ILiteCollection<Person> collection = database.GetCollection<Person>("people");
IQueryable<Person> queryable = collection.AsQueryable();
```

or 

```C#
LiteDatabaseAsync database = new LiteDatabaseAsync("test.db");
ILiteCollectionAsync<Person> collection = database.GetCollection<Person>("people");
IQueryable<Person> queryable = collection.AsQueryable();
```

After that you can use the synchronous LINQ extensions with both variants and the asynchronous
ones with the second variant.

## Supported LINQ extensions

You can use the following methods to configure your query. Those methods are available in both
variants.

- Where
- OrderBy
- OrderByDescending
- Skip
- Take
- Include
- Select

The ```ThenBy/ThenByDescending``` methods and multiple OrderBy/OrderByDescending class are not
supported at the moment, because LiteDB doesn't support multiple order exprssions.

### Synchronous extenions

- ToList
- ToArray
- ToDictionary
- First
- FirstOrDefault
- Single
- SingleOrDefault
- Count
- LongCount
- Any
- Sum
- Average
- Min
- Max

### Asynchronous extenions

- ToListAsync
- ToArrayAsync
- ToDictionaryAsync
- FirstAsync
- FirstOrDefaultAsync
- SingleAsync
- SingleOrDefaultAsync
- CountAsync
- LongCountAsync
- AnyAsync
- SumAsync
- AverageAsync
- MinAsync
- MaxAsync

## Example

```C#
LiteDatabase database = new LiteDatabase("test.db");
ILiteCollection<Person> collection = database.GetCollection<Person>("people");
IQueryable<Person> queryable = collection.AsQueryable();

IList<Person> result = queryable
	.Where(x => x.Name.StartsWith("T"))
	.OrderBy(x => x.Age)
	.ToList();

Person result = queryable
	.Where(x => x.Age > 35)
	.FirstOrDefault();

string result = queryable
	.Where(x => x.Age > 35)
	.Select(x => x.Name)
	.FirstOrDefault();
```

or 

```C#
LiteDatabaseAsync database = new LiteDatabaseAsync("test.db");
ILiteCollectionAsync<Person> collection = database.GetCollection<Person>("people");
IQueryable<Person> queryable = collection.AsQueryable();

IList<Person> result = await queryable
	.Where(x => x.Name.StartsWith("T"))
	.OrderBy(x => x.Age)
	.ToListAsync();

Person result = await queryable
	.Where(x => x.Age > 35)
	.FirstOrDefaultAsync();

string result = await queryable
	.Where(x => x.Age > 35)
	.Select(x => x.Name)
	.FirstOrDefaultAsync();
```

## References

- [LiteDB](https://github.com/mbdavid/LiteDB)
- [LiteDB Async](https://github.com/mlockett42/litedb-async)
- [Entity Framework Core](https://github.com/dotnet/efcore)
