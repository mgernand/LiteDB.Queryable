namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class MinFinder : ExpressionVisitor
	{
		private LambdaExpression minExpression;
		private object comparer;

		public MinResult GetMinExpression(Expression expression)
		{
			this.Visit(expression);
			return new MinResult
			{
				Selector = this.minExpression,
				Comparer = this.comparer
			};
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Min")
			{
				if(node.Arguments.Count == 2)
				{
					Expression argument = node.Arguments[1];

					if(argument is UnaryExpression expression)
					{
						LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
						this.minExpression = lambdaExpression;
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
