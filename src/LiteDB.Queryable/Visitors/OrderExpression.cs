namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class OrderExpression
	{
		public OrderExpression(LambdaExpression expression, bool isDescending)
		{
			this.Expression = expression;
			this.IsDescending = isDescending;
		}

		public LambdaExpression Expression { get; }

		public bool IsDescending { get; }
	}
}
