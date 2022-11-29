namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class SkipFinder : ExpressionVisitor
	{
		private int? skipAmount;

		public int? GetSkipValue(Expression expression)
		{
			this.Visit(expression);

			return this.skipAmount;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Skip")
			{
				if(node.Arguments.Count == 2)
				{
					ConstantExpression expression = (ConstantExpression)node.Arguments[1];
					this.skipAmount = expression.Value as int?;
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
