// ReSharper disable StaticMemberInGenericType

namespace LiteDB.Queryable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using LiteDB.Async;
	using LiteDB.Queryable.Visitors;

	internal sealed class LiteQueryBuilder<T>
	{
		private static readonly Dictionary<Type, MethodInfo> GenericSumMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Sum" && m.IsGenericMethod).ToDictionary(x => x.ReturnType, x => x);
		private static readonly Dictionary<Type, MethodInfo> GenericSumAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "SumAsync" && m.IsGenericMethod).ToDictionary(x => x.ReturnType.GetGenericArguments()[0], x => x);

		private static readonly List<MethodInfo> GenericAverageMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Average" && m.IsGenericMethod).ToList();
		private static readonly List<MethodInfo> GenericAverageAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "AverageAsync" && m.IsGenericMethod).ToList();

		private static readonly List<MethodInfo> GenericMinMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Min" && m.IsGenericMethod).ToList();
		private static readonly List<MethodInfo> GenericMinAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "MinAsync" && m.IsGenericMethod).ToList();

		private static readonly List<MethodInfo> GenericMaxMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Max" && m.IsGenericMethod).ToList();
		private static readonly List<MethodInfo> GenericMaxAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "MaxAsync" && m.IsGenericMethod).ToList();

		private static readonly MethodInfo GenericGetExpressionMethod = typeof(BsonMapper).GetRuntimeMethods().Single(m => m.Name == "GetExpression" && m.IsGenericMethod);

		private static readonly Dictionary<Type, MethodInfo> GenericAsTaskMethods = new Dictionary<Type, MethodInfo>();

		private ILiteQueryable<T> queryable;
		private ILiteQueryableAsync<T> queryableAsync;

		private readonly ILiteCollection<T> collection;
		private readonly ILiteCollectionAsync<T> collectionAsync;

		public LiteQueryBuilder(ILiteCollection<T> collection)
		{
			this.queryable = collection.Query();
			this.collection = collection;
		}

		public LiteQueryBuilder(ILiteCollectionAsync<T> collection)
		{
			this.queryableAsync = collection.Query();
			this.collectionAsync = collection;

			// Extract the underlying collection in case the queryable was created
			// for an async collection but is used with the non-async LINQ methods.
			LiteCollectionAsync<T> liteCollection = (LiteCollectionAsync<T>)collection;
			this.queryable = liteCollection.UnderlyingCollection.Query();
			this.collection = liteCollection.UnderlyingCollection;
		}

		public TResult ExecuteAsync<TResult>(Expression expression)
		{
			return this.Execute<TResult>(expression, true);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			return this.Execute<TResult>(expression, false);
		}

		private TResult Execute<TResult>(Expression expression, bool isAsync)
		{
			if(isAsync && this.collectionAsync is null)
			{
				throw new InvalidOperationException("The queryable is not async.");
			}

			string executionMethodName = GetExecutionMethodName(expression);

			// Apply 'Where' expressions to query.
			this.ApplyWhere(expression, isAsync);

			// Apply 'OrderBy' expressions to query.
			this.ApplyOrderBy(expression, isAsync);

			// Apply 'Skip' value to query.
			this.ApplySkip(expression, isAsync);

			// Apply 'Take' value to query.
			this.ApplyTake(expression, isAsync);

			TResult result = executionMethodName switch
			{
				"root" => this.GetEnumerableResult<TResult>(isAsync),
				nameof(Queryable.First) => this.GetFirstResult<TResult>(isAsync),
				nameof(Queryable.FirstOrDefault) => this.GetFirstOrDefaultResult<TResult>(isAsync),
				nameof(Queryable.Single) => this.GetSingleResult<TResult>(isAsync),
				nameof(Queryable.SingleOrDefault) => this.GetSingleOrDefaultResult<TResult>(isAsync),
				nameof(Queryable.Count) => this.GetCountResult<TResult>(isAsync),
				nameof(Queryable.LongCount) => this.GetLongCountResult<TResult>(isAsync),
				nameof(Queryable.Any) => this.GetAnyResult<TResult>(isAsync),
				nameof(Queryable.Sum) => this.GetSumResult<TResult>(expression, isAsync),
				nameof(Queryable.Average) => this.GetAverageResult<TResult>(expression, isAsync),
				nameof(Queryable.Min) => this.GetMinResult<TResult>(expression, isAsync),
				nameof(Queryable.Max) => this.GetMaxResult<TResult>(expression, isAsync),
				_ => throw new NotSupportedException($"The LINQ extension method '{executionMethodName}' is not supported.")
			};

			return result;
		}

		private TResult GetEnumerableResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable()
				: (TResult)this.queryable.ToEnumerable();
		}

		private TResult GetFirstResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.FirstAsync()
				: (TResult)(object)this.queryable.First();
		}

		private TResult GetFirstOrDefaultResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.FirstOrDefaultAsync()
				: (TResult)(object)this.queryable.FirstOrDefault();
		}

		private TResult GetSingleResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.SingleAsync()
				: (TResult)(object)this.queryable.Single();
		}

		private TResult GetSingleOrDefaultResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.SingleOrDefaultAsync()
				: (TResult)(object)this.queryable.SingleOrDefault();
		}

		private TResult GetCountResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.CountAsync()
				: (TResult)(object)this.queryable.Count();
		}

		private TResult GetLongCountResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.LongCountAsync()
				: (TResult)(object)this.queryable.LongCount();
		}

		private TResult GetAnyResult<TResult>(bool isAsync)
		{
			return isAsync
				? (TResult)(object)this.queryableAsync.ExistsAsync()
				: (TResult)(object)this.queryable.Exists();
		}

		private TResult GetSumResult<TResult>(Expression expression, bool isAsync)
		{
			SumFinder sumFinder = new SumFinder();
			LambdaExpression selector = sumFinder.GetSumExpression(expression);

			TResult result;

			if(isAsync)
			{
				MethodInfo genericSumAsyncMethod = GenericSumAsyncMethods[selector.ReturnType];
				object valueTask = genericSumAsyncMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable(),
						selector.Compile(),
						default(CancellationToken)
					});


				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				MethodInfo genericSumMethod = GenericSumMethods[selector.ReturnType];
				result = (TResult)genericSumMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryable.ToEnumerable().AsQueryable(),
						selector
					});
			}

			return result;
		}

		private TResult GetAverageResult<TResult>(Expression expression, bool isAsync)
		{
			AverageFinder averageFinder = new AverageFinder();
			LambdaExpression selector = averageFinder.GetAverageExpression(expression);

			Type selectorFuncType = selector.Type;
			Type selectorValueType = selectorFuncType.GetGenericArguments()[1];

			TResult result;

			if(isAsync)
			{
				MethodInfo genericAverageAsyncMethod = GenericAverageAsyncMethods
					.Where(x => x.ReturnType.GetGenericArguments()[0] == typeof(TResult).GetGenericArguments()[0])
					.Where(x =>
					{
						Type funcType = x.GetParameters()[1].ParameterType;
						Type valueType = funcType.GetGenericArguments()[1];

						return valueType == selectorValueType;
					})
					.Single();

				object valueTask = genericAverageAsyncMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable(),
						selector.Compile(),
						default(CancellationToken)
					});

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				MethodInfo genericAverageMethod = GenericAverageMethods
					.Where(x => x.ReturnType == typeof(TResult))
					.Where(x =>
					{
						Type expressionType = x.GetParameters()[1].ParameterType;
						Type funcType = expressionType.GetGenericArguments()[0];
						Type valueType = funcType.GetGenericArguments()[1];

						return valueType == selectorValueType;
					})
					.Single();

				result = (TResult)genericAverageMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryable.ToEnumerable().AsQueryable(),
						selector
					});
			}

			return result;
		}

		private TResult GetMinResult<TResult>(Expression expression, bool isAsync)
		{
			MinFinder minFinder = new MinFinder();
			LambdaExpression selector = minFinder.GetMinExpression(expression);

			TResult result;

			if(isAsync)
			{
				MethodInfo genericMinAsyncMethod = GenericMinAsyncMethods
					.Where(x =>
					{
						Type parameterType = x.GetParameters()[1].ParameterType;
						return parameterType.Name == "Func`2" && parameterType.GetGenericArguments()[1] == selector.ReturnType;
					})
					.Single();

				object valueTask = genericMinAsyncMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable(),
						selector.Compile(),
						default(CancellationToken)
					});

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				MethodInfo genericMinMethod = GenericMinMethods
					.Where(x =>
					{
						Type parameterType = x.GetParameters().LastOrDefault()?.ParameterType;
						return parameterType?.Name == "Expression`1";
					})
					.Single();


				result = (TResult)genericMinMethod
					.MakeGenericMethod(typeof(T), typeof(TResult))
					.Invoke(null, new object[]
					{
						this.queryable.ToEnumerable().AsQueryable(),
						selector
					});
			}

			return result;
		}

		private TResult GetMaxResult<TResult>(Expression expression, bool isAsync)
		{
			MaxFinder maxFinder = new MaxFinder();
			LambdaExpression selector = maxFinder.GetMaxExpression(expression);

			TResult result;

			if(isAsync)
			{
				MethodInfo genericMaxAsyncMethod = GenericMaxAsyncMethods
					.Where(x =>
					{
						Type parameterType = x.GetParameters()[1].ParameterType;
						return parameterType.Name == "Func`2" && parameterType.GetGenericArguments()[1] == selector.ReturnType;
					})
					.Single();

				object valueTask = genericMaxAsyncMethod
					.MakeGenericMethod(typeof(T))
					.Invoke(null, new object[]
					{
						this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable(),
						selector.Compile(),
						default(CancellationToken)
					});

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				MethodInfo genericMaxMethod = GenericMaxMethods
					.Where(x =>
					{
						Type parameterType = x.GetParameters().LastOrDefault()?.ParameterType;
						return parameterType?.Name == "Expression`1";
					})
					.Single();


				result = (TResult)genericMaxMethod
					.MakeGenericMethod(typeof(T), typeof(TResult))
					.Invoke(null, new object[]
					{
						this.queryable.ToEnumerable().AsQueryable(),
						selector
					});
			}

			return result;
		}

		private static string GetExecutionMethodName(Expression expression)
		{
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			string executionMethodName = methodCallExpression?.Method.Name;

			// Filter out special method names that need to return the root result.
			executionMethodName = executionMethodName switch
			{
				nameof(Enumerable.Where) => "root",
				nameof(Enumerable.OrderBy) => "root",
				nameof(Enumerable.OrderByDescending) => "root",
				nameof(Enumerable.Skip) => "root",
				nameof(Enumerable.Take) => "root",
				null => "root",
				_ => executionMethodName
			};

			return executionMethodName;
		}

		private void ApplyWhere(Expression expression, bool isAsync)
		{
			foreach(BsonExpression bsonExpression in EnumerateWhereExpressions(expression))
			{
				if(isAsync)
				{
					this.queryableAsync = this.queryableAsync.Where(bsonExpression);
				}
				else
				{
					this.queryable = this.queryable.Where(bsonExpression);
				}
			}
		}

		private void ApplyOrderBy(Expression expression, bool isAsync)
		{
			int orderByCounter = 0;
			foreach((BsonExpression bsonExpression, bool isDescending, bool isSecondary) in EnumerateOrderExpressions(expression))
			{
				if(!isSecondary)
				{
					if(orderByCounter > 0)
					{
						throw new NotSupportedException("Multiple OrderBy is not supported.");
					}

					if(isAsync)
					{
						this.queryableAsync = isDescending
							? this.queryableAsync.OrderByDescending(bsonExpression)
							: this.queryableAsync.OrderBy(bsonExpression);
					}
					else
					{
						this.queryable = isDescending
							? this.queryable.OrderByDescending(bsonExpression)
							: this.queryable.OrderBy(bsonExpression);
					}

					orderByCounter++;
				}
				else
				{
					throw new NotSupportedException("ThenBy is not supported.");
				}
			}
		}

		private void ApplySkip(Expression expression, bool isAsync)
		{
			SkipFinder skipFinder = new SkipFinder();
			int? skipValue = skipFinder.GetSkipValue(expression);
			if(skipValue.HasValue)
			{
				if(isAsync)
				{
					this.queryableAsync = (ILiteQueryableAsync<T>)this.queryableAsync.Skip(skipValue.Value);
				}
				else
				{
					this.queryable = (ILiteQueryable<T>)this.queryable.Skip(skipValue.Value);
				}
			}
		}

		private void ApplyTake(Expression expression, bool isAsync)
		{
			TakeFinder takeFinder = new TakeFinder();
			int? takeAmount = takeFinder.GetTakeValue(expression);
			if(takeAmount.HasValue)
			{
				if(isAsync)
				{
					this.queryableAsync = (ILiteQueryableAsync<T>)this.queryableAsync.Limit(takeAmount.Value);
				}
				else
				{
					this.queryable = (ILiteQueryable<T>)this.queryable.Limit(takeAmount.Value);
				}
			}
		}

		private static IEnumerable<BsonExpression> EnumerateWhereExpressions(Expression expression)
		{
			WhereFinder whereFinder = new WhereFinder();
			IList<LambdaExpression> whereExpressions = whereFinder.GetWhereExpressions(expression);

			foreach(LambdaExpression whereExpression in whereExpressions)
			{
				BsonExpression bsonExpression = BsonMapper.Global.GetExpression((Expression<Func<T, bool>>)whereExpression);
				yield return bsonExpression;
			}
		}

		private static IEnumerable<(BsonExpression, bool, bool)> EnumerateOrderExpressions(Expression expression)
		{
			OrderByFinder whereFinder = new OrderByFinder();
			OrderResult orderResult = whereFinder.GetOrderByExpressions(expression);

			foreach(OrderExpression orderByExpression in orderResult.OrderByExpressions)
			{
				BsonExpression bsonExpression = (BsonExpression)GenericGetExpressionMethod
					.MakeGenericMethod(typeof(T), orderByExpression.Expression.ReturnType)
					.Invoke(BsonMapper.Global, new object[]
					{
						orderByExpression.Expression
					});

				yield return (bsonExpression, orderByExpression.IsDescending, false);
			}

			foreach(OrderExpression orderByExpression in orderResult.ThenByExpressions)
			{
				BsonExpression bsonExpression = (BsonExpression)GenericGetExpressionMethod
					.MakeGenericMethod(typeof(T), orderByExpression.Expression.ReturnType)
					.Invoke(BsonMapper.Global, new object[]
					{
						orderByExpression.Expression
					});

				yield return (bsonExpression, orderByExpression.IsDescending, true);
			}
		}

		private static MethodInfo GetAsTaskMethodFor<TResult>()
		{
			MethodInfo methodInfo;

			Type resultType = typeof(TResult);
			if(resultType.Name == "Task`1")
			{
				resultType = resultType.GetGenericArguments()[0];
			}

			if(GenericAsTaskMethods.ContainsKey(resultType))
			{
				methodInfo = GenericAsTaskMethods[resultType];
			}
			else
			{
				methodInfo = typeof(ValueTask<>).MakeGenericType(resultType).GetRuntimeMethods().Single(m => m.Name == "AsTask");
				GenericAsTaskMethods.Add(resultType, methodInfo);
			}

			return methodInfo;
		}
	}
}
