namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class TakeFinder : ExpressionVisitor
	{
		private int? takeAmount;

		public int? GetTakeValue(Expression expression)
		{
			this.Visit(expression);

			return this.takeAmount;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Take")
			{
				if(node.Arguments.Count == 2)
				{
					ConstantExpression expression = (ConstantExpression)node.Arguments[1];
					this.takeAmount = expression.Value as int?;
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
