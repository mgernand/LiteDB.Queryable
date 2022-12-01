namespace LiteDB.Queryable
{
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using LiteDB.Async;

	internal sealed class LiteQueryProvider<T> : IAsyncQueryProvider
	{
		private static readonly MethodInfo GenericCreateQueryMethod = typeof(LiteQueryProvider<T>).GetRuntimeMethods().Single(m => m.Name == "CreateQuery" && m.IsGenericMethod);
		private static readonly MethodInfo GenericExecuteMethod = typeof(LiteQueryProvider<T>).GetRuntimeMethods().Single(m => m.Name == "Execute" && m.IsGenericMethod);

		private readonly LiteQueryBuilder<T> queryBuilder;

		public LiteQueryProvider(ILiteCollection<T> collection)
		{
			this.queryBuilder = new LiteQueryBuilder<T>(collection);
		}

		public LiteQueryProvider(ILiteCollectionAsync<T> collection)
		{
			this.queryBuilder = new LiteQueryBuilder<T>(collection);
		}

		/// <inheritdoc />
		public IQueryable CreateQuery(Expression expression)
		{
			IQueryable queryable = (IQueryable)GenericCreateQueryMethod
				.MakeGenericMethod(expression.Type.GetSequenceType())
				.Invoke(this, new object[]
				{
					expression
				});

			return queryable;
		}

		/// <inheritdoc />
		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			IQueryable<TElement> queryable = new LiteQueryable<TElement>(this, expression);

			return queryable;
		}

		/// <inheritdoc />
		public object Execute(Expression expression)
		{
			object result = GenericExecuteMethod
				.MakeGenericMethod(expression.Type)
				.Invoke(this, new object[]
				{
					expression
				});

			return result;
		}

		/// <inheritdoc />
		public TResult Execute<TResult>(Expression expression)
		{
			return this.queryBuilder.Execute<TResult>(expression);
		}

		/// <inheritdoc />
		public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
		{
			return this.queryBuilder.ExecuteAsync<TResult>(expression);
		}
	}
}
