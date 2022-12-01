namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class OrderByFinder : ExpressionVisitor
	{
		private readonly List<OrderExpression> orderByExpressions = new List<OrderExpression>();
		private readonly List<OrderExpression> thenByExpressions = new List<OrderExpression>();

		public OrderResult GetOrderByExpressions(Expression expression)
		{
			this.Visit(expression);

			this.orderByExpressions.Reverse();
			this.thenByExpressions.Reverse();

			OrderResult result = new OrderResult
			{
				OrderByExpressions = this.orderByExpressions,
				ThenByExpressions = this.thenByExpressions
			};

			return result;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "OrderBy" or "OrderByDescending")
			{
				if(node.Arguments.Count == 2)
				{
					bool isDescending = node.Method.Name.EndsWith("Descending");

					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.orderByExpressions.Add(new OrderExpression(lambdaExpression, isDescending));
				}
			}

			if(node.Method.Name is "ThenBy" or "ThenByDescending")
			{
				if(node.Arguments.Count == 2)
				{
					bool isDescending = node.Method.Name.EndsWith("Descending");

					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.thenByExpressions.Add(new OrderExpression(lambdaExpression, isDescending));
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
