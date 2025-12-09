namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class SelectFinder : ExpressionVisitor
	{
		private bool isAggregateMethodWithoutSelectorUsed;
		private readonly List<LambdaExpression> selectExpressions = [];

		public SelectResult GetSelectExpressions(Expression expression)
		{
			this.Visit(expression);
			this.selectExpressions.Reverse();

			return new SelectResult
			{
				Expressions = this.selectExpressions,
				IsAggregateMethodWithoutSelectorUsed = this.isAggregateMethodWithoutSelectorUsed
			};
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Select")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.selectExpressions.Add(lambdaExpression);
				}
			}

			if(node.Method.Name is "Sum" or "Average")
			{
				this.isAggregateMethodWithoutSelectorUsed = node.Arguments.Count == 1;
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
