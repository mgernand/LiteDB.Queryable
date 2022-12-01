namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class MaxResult
	{
		public LambdaExpression Selector { get; set; }

		public object Comparer { get; set; }
	}
}
