namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class IncludeFinder : ExpressionVisitor
	{
		private readonly List<LambdaExpression> includeExpressions = [];

		public IList<LambdaExpression> GetIncludeExpressions(Expression expression)
		{
			this.Visit(expression);
			this.includeExpressions.Reverse();
			return this.includeExpressions;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Include")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.includeExpressions.Add(lambdaExpression);
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
