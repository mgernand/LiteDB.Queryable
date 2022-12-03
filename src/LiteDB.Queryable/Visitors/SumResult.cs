namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class SumResult
	{
		public LambdaExpression Expression { get; set; }

		public LambdaExpression SelectExpression { get; set; }
	}
}
