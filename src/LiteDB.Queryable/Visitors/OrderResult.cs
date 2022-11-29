namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;

	internal sealed class OrderResult
	{
		public IList<OrderExpression> OrderByExpressions { get; set; }

		public IList<OrderExpression> ThenByExpressions { get; set; }
	}
}
