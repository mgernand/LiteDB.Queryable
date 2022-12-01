// ReSharper disable StaticMemberInGenericType

// ReSharper disable ReplaceWithSingleCallToSingle

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
		private static readonly Dictionary<Type, MethodInfo> GenericAsTaskMethods = new Dictionary<Type, MethodInfo>();
		private static readonly MethodInfo GenericGetExpressionMethod = typeof(BsonMapper).GetRuntimeMethods().Single(m => m.Name == "GetExpression" && m.IsGenericMethod);

		private static readonly List<MethodInfo> SumMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Sum").ToList();
		private static readonly List<MethodInfo> SumAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "SumAsync").ToList();

		private static readonly List<MethodInfo> AverageMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Average").ToList();
		private static readonly List<MethodInfo> AverageAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "AverageAsync").ToList();

		private static readonly List<MethodInfo> MinMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Min").ToList();
		private static readonly List<MethodInfo> MinAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "MinAsync").ToList();

		private static readonly List<MethodInfo> MaxMethods = typeof(Queryable).GetRuntimeMethods().Where(m => m.Name == "Max").ToList();
		private static readonly List<MethodInfo> MaxAsyncMethods = typeof(AsyncEnumerable).GetRuntimeMethods().Where(m => m.Name == "MaxAsync").ToList();

		private static readonly MethodInfo GenericSelectMethod = typeof(ILiteQueryable<T>).GetRuntimeMethods().Single(m => m.Name == "Select" && m.IsGenericMethod);
		private static readonly MethodInfo GenericSelectAsyncMethod = typeof(ILiteQueryableAsync<T>).GetRuntimeMethods().Single(m => m.Name == "Select" && m.IsGenericMethod);

		private static readonly MethodInfo GenericIncludeMethod = typeof(ILiteQueryable<T>).GetRuntimeMethods().Single(m => m.Name == "Include" && m.IsGenericMethod);
		private static readonly MethodInfo GenericIncludeAsyncMethod = typeof(ILiteQueryableAsync<T>).GetRuntimeMethods().Single(m => m.Name == "Include" && m.IsGenericMethod);

		private static readonly MethodInfo GenericAsQueryableMethod = typeof(Queryable).GetRuntimeMethods().Single(m => m.Name == "AsQueryable" && m.IsGenericMethod);
		private static readonly MethodInfo GenericAsAsyncEnumerableMethod = typeof(QueryableExtensions).GetRuntimeMethods().Single(m => m.Name == "AsAsyncEnumerable" && m.IsGenericMethod);

		private ILiteQueryable<T> queryable;
		private ILiteQueryableAsync<T> queryableAsync;

		private bool isSelectApplied;
		private object selectedQueryable;
		private object selectedQueryableAsync;

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

			// Apply 'Where' expression(s) to query.
			this.ApplyWhere(expression, isAsync);

			// Apply 'OrderBy' expression(s) to query.
			this.ApplyOrderBy(expression, isAsync);

			// Apply 'Skip' value to query.
			this.ApplySkip(expression, isAsync);

			// Apply 'Take' value to query.
			this.ApplyTake(expression, isAsync);

			// Apply 'Include' expression(s) to query.
			this.ApplyInclude(expression, isAsync);

			// Apply 'Select' expression(s) to query.
			this.ApplySelect(expression, isAsync);

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
			bool isSelectorApplied = selector is not null;

			TResult result;

			if(isAsync)
			{
				object valueTask;

				if(isSelectorApplied)
				{
					MethodInfo sumAsyncMethod = SumAsyncMethods
						.Where(x => x.IsGenericMethod)
						.Where(x => x.ReturnType.GetGenericArguments()[0] == typeof(TResult).GetGenericArguments()[0])
						.Single();

					valueTask = sumAsyncMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableAsyncInstance(),
							selector.Compile(),
							default(CancellationToken)
						});
				}
				else
				{
					MethodInfo sumAsyncMethod = SumAsyncMethods
						.Where(x => !x.IsGenericMethod)
						.Where(x => x.ReturnType.GetGenericArguments()[0] == typeof(TResult).GetGenericArguments()[0])
						.Single();

					valueTask = sumAsyncMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableAsyncInstance(),
							default(CancellationToken)
						});
				}

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				if(isSelectorApplied)
				{
					MethodInfo sumAsyncMethod = SumMethods
						.Where(x => x.IsGenericMethod)
						.Where(x => x.ReturnType == typeof(TResult))
						.Single();

					result = (TResult)sumAsyncMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableInstance(),
							selector
						});
				}
				else
				{
					MethodInfo sumAsyncMethod = SumMethods
						.Where(x => !x.IsGenericMethod)
						.Where(x => x.ReturnType == typeof(TResult))
						.Single();

					result = (TResult)sumAsyncMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableInstance()
						});
				}
			}

			return result;
		}

		private TResult GetAverageResult<TResult>(Expression expression, bool isAsync)
		{
			AverageFinder averageFinder = new AverageFinder();
			LambdaExpression selector = averageFinder.GetAverageExpression(expression);
			bool isSelectorApplied = selector is not null;

			TResult result;

			if(isAsync)
			{
				object valueTask;

				if(isSelectorApplied)
				{
					Type selectorFuncType = selector.Type;
					Type selectorValueType = selectorFuncType.GetGenericArguments()[1];

					MethodInfo averageAsyncMethod = AverageAsyncMethods
						.Where(x => x.IsGenericMethod)
						.Where(x => x.ReturnType.GetGenericArguments()[0] == typeof(TResult).GetGenericArguments()[0])
						.Where(x =>
						{
							Type funcType = x.GetParameters()[1].ParameterType;
							Type valueType = funcType.GetGenericArguments()[1];

							return valueType == selectorValueType;
						})
						.Single();

					valueTask = averageAsyncMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableAsyncInstance(),
							selector.Compile(),
							default(CancellationToken)
						});
				}
				else
				{
					Type genericArgumentType = this.selectedQueryableAsync.GetType().GetGenericArguments().Single();

					MethodInfo averageAsyncMethod = AverageAsyncMethods
						.Where(x => x.ReturnType.GetGenericArguments()[0] == typeof(TResult).GetGenericArguments()[0])
						.Where(x => x.GetParameters().Length == 2)
						.Where(x =>
						{
							Type funcType = x.GetParameters()[0].ParameterType;
							Type valueType = funcType.GetGenericArguments()[0];

							return valueType == genericArgumentType;
						})
						.Single();

					valueTask = averageAsyncMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableAsyncInstance(),
							default(CancellationToken)
						});
				}

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				if(isSelectorApplied)
				{
					Type selectorFuncType = selector.Type;
					Type selectorValueType = selectorFuncType.GetGenericArguments()[1];

					MethodInfo averageMethod = AverageMethods
						.Where(x => x.IsGenericMethod)
						.Where(x => x.ReturnType == typeof(TResult))
						.Where(x =>
						{
							Type expressionType = x.GetParameters()[1].ParameterType;
							Type funcType = expressionType.GetGenericArguments()[0];
							Type valueType = funcType.GetGenericArguments()[1];

							return valueType == selectorValueType;
						})
						.Single();

					result = (TResult)averageMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableInstance(),
							selector
						});
				}
				else
				{
					Type genericArgumentType = this.selectedQueryable.GetType().GetGenericArguments().Single();

					MethodInfo averageMethod = AverageMethods
						.Where(x => x.ReturnType == typeof(TResult))
						.Where(x =>
						{
							Type funcType = x.GetParameters()[0].ParameterType;
							Type valueType = funcType.GetGenericArguments()[0];

							return valueType == genericArgumentType;
						})
						.Single();

					result = (TResult)averageMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableInstance()
						});
				}
			}

			return result;
		}

		private TResult GetMinResult<TResult>(Expression expression, bool isAsync)
		{
			MinFinder minFinder = new MinFinder();
			MinResult minResult = minFinder.GetMinExpression(expression);
			LambdaExpression selector = minResult.Selector;
			object comparer = minResult.Comparer;
			bool isSelectorApplied = selector is not null;

			TResult result;

			if(isAsync)
			{
				object valueTask;

				if(isSelectorApplied)
				{
					MethodInfo minAsyncMethod = MinAsyncMethods
						.Where(x => x.IsGenericMethod)
						.Where(x =>
						{
							Type parameterType = x.GetParameters()[1].ParameterType;
							return parameterType.Name == "Func`2" && parameterType.GetGenericArguments()[1] == selector.ReturnType;
						})
						.Single();

					valueTask = minAsyncMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableAsyncInstance(),
							selector.Compile(),
							default(CancellationToken)
						});
				}
				else
				{
					Type resultType = typeof(TResult).GetGenericArguments().Single();
					Type genericArgumentType = this.selectedQueryableAsync?.GetType().GetGenericArguments().Single();

					MethodInfo minAsyncMethod = MinAsyncMethods
						.Where(x => x.GetParameters().Length == 2)
						.Where(x =>
						{
							Type funcType = x.GetParameters()[0].ParameterType;
							Type valueType = funcType.GetGenericArguments()[0];

							return genericArgumentType is not null
								? valueType == genericArgumentType
								: !valueType.IsPrimitive && !valueType.IsNullable() && valueType != typeof(decimal);
						})
						.Single();

					if(!this.isSelectApplied)
					{
						minAsyncMethod = minAsyncMethod
							.MakeGenericMethod(resultType);
					}

					valueTask = minAsyncMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableAsyncInstance() ?? this.GetEnumerableAsyncInstance(),
							default(CancellationToken)
						});
				}

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				if(isSelectorApplied)
				{
					MethodInfo minMethod = MinMethods
						.Where(x => x.IsGenericMethod)
						.Where(x =>
						{
							Type parameterType = x.GetParameters().LastOrDefault()?.ParameterType;
							return parameterType?.Name == "Expression`1";
						})
						.Single();


					result = (TResult)minMethod
						.MakeGenericMethod(typeof(T), typeof(TResult))
						.Invoke(null, new object[]
						{
							this.GetEnumerableInstance(),
							selector
						});
				}
				else
				{
					MethodInfo minMethod = MinMethods
						.Where(x =>
						{
							ParameterInfo[] parameterInfos = x.GetParameters();
							return comparer is null
								? parameterInfos.Length == 1
								: parameterInfos.Length == 2 && parameterInfos.Last().ParameterType.IsComparer();
						})
						.Single();

					IList<object> parameters = new List<object>
					{
						this.GetSelectEnumerableInstance() ?? this.GetEnumerableInstance()
					};

					if(comparer is not null)
					{
						parameters.Add(comparer);
					}

					result = (TResult)minMethod
						.MakeGenericMethod(typeof(TResult))
						.Invoke(null, parameters.ToArray());
				}
			}

			return result;
		}

		private TResult GetMaxResult<TResult>(Expression expression, bool isAsync)
		{
			MaxFinder maxFinder = new MaxFinder();
			MaxResult maxResult = maxFinder.GetMaxExpression(expression);
			LambdaExpression selector = maxResult.Selector;
			object comparer = maxResult.Comparer;
			bool isSelectorApplied = selector is not null;

			TResult result;

			if(isAsync)
			{
				object valueTask;

				if(isSelectorApplied)
				{
					MethodInfo maxAsyncMethod = MaxAsyncMethods
						.Where(x =>
						{
							Type parameterType = x.GetParameters()[1].ParameterType;
							return parameterType.Name == "Func`2" && parameterType.GetGenericArguments()[1] == selector.ReturnType;
						})
						.Single();

					valueTask = maxAsyncMethod
						.MakeGenericMethod(typeof(T))
						.Invoke(null, new object[]
						{
							this.GetEnumerableAsyncInstance(),
							selector.Compile(),
							default(CancellationToken)
						});
				}
				else
				{
					Type resultType = typeof(TResult).GetGenericArguments().Single();
					Type genericArgumentType = this.selectedQueryableAsync?.GetType().GetGenericArguments().Single();

					MethodInfo maxAsyncMethod = MaxAsyncMethods
						.Where(x => x.GetParameters().Length == 2)
						.Where(x =>
						{
							Type funcType = x.GetParameters()[0].ParameterType;
							Type valueType = funcType.GetGenericArguments()[0];

							return genericArgumentType is not null
								? valueType == genericArgumentType
								: !valueType.IsPrimitive && !valueType.IsNullable() && valueType != typeof(decimal);
						})
						.Single();

					if(!this.isSelectApplied)
					{
						maxAsyncMethod = maxAsyncMethod
							.MakeGenericMethod(resultType);
					}

					valueTask = maxAsyncMethod
						.Invoke(null, new object[]
						{
							this.GetSelectEnumerableAsyncInstance() ?? this.GetEnumerableAsyncInstance(),
							default(CancellationToken)
						});
				}

				MethodInfo asTaskMethod = GetAsTaskMethodFor<TResult>();
				result = (TResult)asTaskMethod.Invoke(valueTask, Array.Empty<object>());
			}
			else
			{
				if(isSelectorApplied)
				{
					MethodInfo maxMethod = MaxMethods
						.Where(x =>
						{
							Type parameterType = x.GetParameters().LastOrDefault()?.ParameterType;
							return parameterType?.Name == "Expression`1";
						})
						.Single();


					result = (TResult)maxMethod
						.MakeGenericMethod(typeof(T), typeof(TResult))
						.Invoke(null, new object[]
						{
							this.GetEnumerableInstance(),
							selector
						});
				}
				else
				{
					MethodInfo maxMethod = MaxMethods
						.Where(x =>
						{
							ParameterInfo[] parameterInfos = x.GetParameters();
							return comparer is null
								? parameterInfos.Length == 1
								: parameterInfos.Length == 2 && parameterInfos.Last().ParameterType.IsComparer();
						})
						.Single();

					IList<object> parameters = new List<object>
					{
						this.GetSelectEnumerableInstance() ?? this.GetEnumerableInstance()
					};

					if(comparer is not null)
					{
						parameters.Add(comparer);
					}

					result = (TResult)maxMethod
						.MakeGenericMethod(typeof(TResult))
						.Invoke(null, parameters.ToArray());
				}
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
			foreach(BsonExpression whereExpression in EnumerateWhereExpressions(expression))
			{
				if(isAsync)
				{
					this.queryableAsync = this.queryableAsync.Where(whereExpression);
				}
				else
				{
					this.queryable = this.queryable.Where(whereExpression);
				}
			}
		}

		private void ApplyOrderBy(Expression expression, bool isAsync)
		{
			int orderByCounter = 0;
			foreach((BsonExpression orderExpression, bool isDescending, bool isSecondary) in EnumerateOrderExpressions(expression))
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
							? this.queryableAsync.OrderByDescending(orderExpression)
							: this.queryableAsync.OrderBy(orderExpression);
					}
					else
					{
						this.queryable = isDescending
							? this.queryable.OrderByDescending(orderExpression)
							: this.queryable.OrderBy(orderExpression);
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

		private void ApplyInclude(Expression expression, bool isAsync)
		{
			foreach(LambdaExpression includeExpression in EnumerateIncludeExpressions(expression))
			{
				Type returnType = includeExpression.ReturnType;

				if(isAsync)
				{
					this.queryableAsync = (ILiteQueryableAsync<T>)GenericIncludeAsyncMethod
						.MakeGenericMethod(returnType)
						.Invoke(this.queryableAsync, new object[]
						{
							includeExpression
						});
				}
				else
				{
					this.queryable = (ILiteQueryable<T>)GenericIncludeMethod
						.MakeGenericMethod(returnType)
						.Invoke(this.queryable, new object[]
						{
							includeExpression
						});
				}
			}
		}

		private void ApplySelect(Expression expression, bool isAsync)
		{
			int selectCounter = 0;
			foreach(LambdaExpression selectExpression in EnumerateSelectExpressions(expression))
			{
				if(selectCounter > 0)
				{
					throw new NotSupportedException("Multiple Select is not supported.");
				}

				Type returnType = selectExpression.ReturnType;

				if(isAsync)
				{
					this.selectedQueryableAsync = GenericSelectAsyncMethod
						.MakeGenericMethod(returnType)
						.Invoke(this.queryableAsync, new object[]
						{
							selectExpression
						});
				}
				else
				{
					this.selectedQueryable = GenericSelectMethod
						.MakeGenericMethod(returnType)
						.Invoke(this.queryable, new object[]
						{
							selectExpression
						});
				}

				this.isSelectApplied = true;
				selectCounter++;
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

		private static IEnumerable<LambdaExpression> EnumerateIncludeExpressions(Expression expression)
		{
			IncludeFinder includeFinder = new IncludeFinder();
			IList<LambdaExpression> includeExpressions = includeFinder.GetIncludeExpressions(expression);

			foreach(LambdaExpression includeExpression in includeExpressions)
			{
				yield return includeExpression;
			}
		}

		private static IEnumerable<LambdaExpression> EnumerateSelectExpressions(Expression expression)
		{
			SelectFinder whereFinder = new SelectFinder();
			IList<LambdaExpression> selectExpressions = whereFinder.GetSelectExpressions(expression);

			foreach(LambdaExpression selectExpression in selectExpressions)
			{
				yield return selectExpression;
			}
		}

		private object GetEnumerableAsyncInstance()
		{
			return this.queryableAsync.ToEnumerableAsync().AsAsyncEnumerable();
		}

		private object GetEnumerableInstance()
		{
			return this.queryable.ToEnumerable().AsQueryable();
		}

		private object GetSelectEnumerableAsyncInstance()
		{
			object enumerable = null;

			Type genericArgumentType = this.selectedQueryableAsync?.GetType().GetGenericArguments().Single();
			if(genericArgumentType is not null)
			{
				MethodInfo toEnumerableAsyncMethod = typeof(ILiteQueryableAsyncResult<>).MakeGenericType(genericArgumentType).GetRuntimeMethods().Single(m => m.Name == "ToEnumerableAsync");
				enumerable = toEnumerableAsyncMethod.Invoke(this.selectedQueryableAsync, Array.Empty<object>());

				enumerable = GenericAsAsyncEnumerableMethod
					.MakeGenericMethod(genericArgumentType)
					.Invoke(null, new[]
					{
						enumerable
					});
			}

			return enumerable;
		}

		private object GetSelectEnumerableInstance()
		{
			object enumerable = null;

			Type genericArgumentType = this.selectedQueryable?.GetType().GetGenericArguments().Single();
			if(genericArgumentType is not null)
			{
				MethodInfo toEnumerableMethod = typeof(ILiteQueryableResult<>).MakeGenericType(genericArgumentType).GetRuntimeMethods().Single(m => m.Name == "ToEnumerable");
				enumerable = toEnumerableMethod.Invoke(this.selectedQueryable, Array.Empty<object>());

				enumerable = GenericAsQueryableMethod
					.MakeGenericMethod(genericArgumentType)
					.Invoke(null, new[]
					{
						enumerable
					});
			}

			return enumerable;
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
