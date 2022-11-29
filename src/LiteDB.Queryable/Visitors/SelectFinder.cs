namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class SelectFinder : ExpressionVisitor
	{
		private readonly List<LambdaExpression> selectExpressions = new List<LambdaExpression>();

		public IList<LambdaExpression> GetSelectExpressions(Expression expression)
		{
			this.Visit(expression);
			this.selectExpressions.Reverse();
			return this.selectExpressions;
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

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
