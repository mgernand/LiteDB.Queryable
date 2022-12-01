namespace LiteDB.Queryable
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using JetBrains.Annotations;
	using LiteDB.Async;

	/// <summary>
	///     Extension methods to create <see cref="IQueryable{T}" /> instances from LiteDB collections.
	/// </summary>
	[PublicAPI]
	public static class QueryableExtensions
	{
		/// <summary>
		///     Creates an <see cref="IQueryable{T}" /> instance from a <see cref="ILiteCollection{T}" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static IQueryable<T> AsQueryable<T>(this ILiteCollection<T> collection)
		{
			IAsyncQueryProvider provider = new LiteQueryProvider<T>(collection);
			IQueryable<T> queryable = new LiteQueryable<T>(provider);

			return queryable;
		}

		/// <summary>
		///     Creates an <see cref="IQueryable{T}" /> instance from a <see cref="ILiteCollectionAsync{T}" />.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static IQueryable<T> AsQueryable<T>(this ILiteCollectionAsync<T> collection)
		{
			IAsyncQueryProvider provider = new LiteQueryProvider<T>(collection);
			IQueryable<T> queryable = new LiteQueryable<T>(provider);

			return queryable;
		}

		internal static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this Task<IEnumerable<T>> task)
		{
			foreach(T item in await task)
			{
				yield return item;
			}
		}
	}
}
