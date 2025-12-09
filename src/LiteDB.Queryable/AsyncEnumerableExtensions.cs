extern alias SystemLinqAsyncLibrary;

namespace LiteDB.Queryable
{
	using System.Collections.Generic;
	using AsyncEnumerable = SystemLinqAsyncLibrary::System.Linq.AsyncEnumerable;

	internal static class AsyncEnumerableExtensions
	{
		public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
		{
			return AsyncEnumerable.ToAsyncEnumerable(source);
		}
	}
}
