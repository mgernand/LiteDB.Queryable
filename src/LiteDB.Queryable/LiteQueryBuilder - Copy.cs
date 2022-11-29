//namespace LiteDB.Linq
//{
//	using System.Linq.Expressions;

//	public class LiteQueryBuilder<T>
//	{
//		private readonly ILiteCollection<T> collection;

//		public LiteQueryBuilder(ILiteCollection<T> collection)
//		{
//			this.collection = collection;
//		}

//		public TResult Execute<TResult>(Expression expression)
//		{
//			//Apply 'Select' expressions.
//			//object selectedQueryable = null;
//			//int selectCounter = 0;
//			//foreach (LambdaExpression selectExpression in EnumerateSelectExpressions(expression))
//			//{
//			//	if (selectCounter > 0)
//			//	{
//			//		throw new NotSupportedException("Multiple Select is not supported.");
//			//	}

//			//	Type unwrappedResultType = typeof(TResult).GetSequenceType();

//			//	if (unwrappedResultType != selectExpression.ReturnType)
//			//	{
//			//		throw new NotSupportedException("Select is not supported in combination with Count, LongCount and Any.");
//			//	}

//			//	MethodInfo genericSelectMethod = typeof(ILiteQueryable<T>).GetRuntimeMethods().Single(m => m.Name == "Select" && m.IsGenericMethod);
//			//	selectedQueryable = genericSelectMethod
//			//		.MakeGenericMethod(unwrappedResultType)
//			//		.Invoke(queryable, new object[]
//			//		{
//			//			selectExpression
//			//		});

//			//	selectCounter++;
//			//}

//			//IEnumerable GetResultEnumerable()
//			//{
//			//	IEnumerable result;

//			//	if (selectedQueryable is not null)
//			//	{
//			//		Type unwrappedResultType = typeof(TResult).GetSequenceType();
//			//		Type liteQueryableTypeOfUnwrappedResult = typeof(ILiteQueryableResult<>).MakeGenericType(unwrappedResultType);
//			//		MethodInfo toEnumerableMethod = liteQueryableTypeOfUnwrappedResult.GetRuntimeMethods().Single(m => m.Name == "ToEnumerable");
//			//		result = (IEnumerable)toEnumerableMethod.Invoke(selectedQueryable, Array.Empty<object>());
//			//	}
//			//	else
//			//	{
//			//		result = queryable.ToEnumerable();
//			//	}

//			//	return result;
//			//}

//		}

//		//private static IEnumerable<LambdaExpression> EnumerateSelectExpressions(Expression expression)
//		//{
//		//	SelectFinder whereFinder = new SelectFinder();
//		//	IList<LambdaExpression> selectExpressions = whereFinder.GetSelectExpressions(expression);

//		//	foreach(LambdaExpression selectExpression in selectExpressions)
//		//	{
//		//		//MethodInfo genericGetExpressionMethod = typeof(BsonMapper).GetRuntimeMethods().Single(m => m.Name == "GetExpression" && m.IsGenericMethod);
//		//		//BsonExpression bsonExpression = (BsonExpression)genericGetExpressionMethod
//		//		//	.MakeGenericMethod(typeof(T), selectExpression.ReturnType)
//		//		//	.Invoke(BsonMapper.Global, new object[]
//		//		//	{
//		//		//		selectExpression
//		//		//	});

//		//		yield return selectExpression;
//		//	}
//		//}

//	}
//}
