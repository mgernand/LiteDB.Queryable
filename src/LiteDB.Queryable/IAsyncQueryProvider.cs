namespace LiteDB.Queryable
{
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading;
	using JetBrains.Annotations;

	/// <summary>
	///     Defines additional methods to execute queries that are described by an
	///     <see cref="IQueryable" /> object asynchronously.
	/// </summary>
	/// <inheritdoc cref="IQueryProvider" />
	[PublicAPI]
	public interface IAsyncQueryProvider : IQueryProvider
	{
		/// <summary>
		///     Executes the strongly-typed query represented by a specified expression tree asynchronously.
		/// </summary>
		TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
	}
}
