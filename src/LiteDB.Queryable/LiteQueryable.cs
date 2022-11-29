namespace LiteDB.Queryable
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading;

	internal sealed class LiteQueryable<TResult> : IOrderedQueryable<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncQueryProvider queryProvider;

		public LiteQueryable(IAsyncQueryProvider provider, Expression expression)
		{
			this.queryProvider = provider;
			this.Expression = expression;
		}

		public LiteQueryable(IAsyncQueryProvider provider)
		{
			this.queryProvider = provider;
			this.Expression = Expression.Constant(this);
		}

		/// <inheritdoc />
		public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
		{
			return this.queryProvider.ExecuteAsync<IAsyncEnumerable<TResult>>(this.Expression, cancellationToken).GetAsyncEnumerator(cancellationToken);
		}

		/// <inheritdoc />
		public IEnumerator<TResult> GetEnumerator()
		{
			return this.queryProvider.Execute<IEnumerable<TResult>>(this.Expression).GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.queryProvider.Execute<IEnumerable>(this.Expression).GetEnumerator();
		}

		/// <inheritdoc />
		public Type ElementType => typeof(TResult);

		/// <inheritdoc />
		public Expression Expression { get; }

		/// <inheritdoc />
		public IQueryProvider Provider => this.queryProvider;
	}
}
