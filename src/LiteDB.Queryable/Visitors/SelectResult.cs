namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class SelectResult
	{
		public IList<LambdaExpression> Expressions { get; set; }

		public bool IsAggregateMethodWithoutSelectorUsed { get; set; }
	}
}
