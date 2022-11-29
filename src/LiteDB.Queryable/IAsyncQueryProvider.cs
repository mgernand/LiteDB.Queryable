namespace LiteDB.Queryable
{
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading;

	public interface IAsyncQueryProvider : IQueryProvider
	{
		/// <summary>
		///     Executes the strongly-typed query represented by a specified expression tree asynchronously.
		/// </summary>
		TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
	}
}
