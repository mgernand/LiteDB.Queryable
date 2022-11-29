namespace LiteDB.Queryable
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using JetBrains.Annotations;
	using LiteDB.Async;

	[PublicAPI]
	public static class QueryableExtensions
	{
		public static IQueryable<T> AsQueryable<T>(this ILiteCollection<T> collection)
		{
			IAsyncQueryProvider provider = new LiteQueryProvider<T>(collection);
			IQueryable<T> queryable = new LiteQueryable<T>(provider);

			return queryable;
		}

		public static IQueryable<T> AsQueryable<T>(this ILiteCollectionAsync<T> collection)
		{
			IAsyncQueryProvider provider = new LiteQueryProvider<T>(collection);
			IQueryable<T> queryable = new LiteQueryable<T>(provider);

			return queryable;
		}

		internal static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<IEnumerable<T>> task)
		{
			foreach(T item in await task)
			{
				yield return item;
			}
		}
	}
}
