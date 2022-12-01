namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class MaxFinder : ExpressionVisitor
	{
		private LambdaExpression maxExpression;
		private object comparer;

		public MaxResult GetMaxExpression(Expression expression)
		{
			this.Visit(expression);
			return new MaxResult
			{
				Selector = this.maxExpression,
				Comparer = this.comparer
			};
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Max")
			{
				if(node.Arguments.Count == 2)
				{
					Expression argument = node.Arguments[1];

					if(argument is UnaryExpression expression)
					{
						LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
						this.maxExpression = lambdaExpression;
					}
					else if(argument is ConstantExpression constantExpression)
					{
						this.comparer = constantExpression.Value;
					}
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
