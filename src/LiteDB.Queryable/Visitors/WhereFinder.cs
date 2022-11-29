namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq.Expressions;

	internal sealed class WhereFinder : ExpressionVisitor
	{
		private readonly List<LambdaExpression> whereExpressions = new List<LambdaExpression>();

		public IList<LambdaExpression> GetWhereExpressions(Expression expression)
		{
			this.Visit(expression);
			this.whereExpressions.Reverse();
			return this.whereExpressions;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Where" or "First" or "FirstOrDefault" or "Single" or "SingleOrDefault" or "Count" or "LongCount" or "Any")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.whereExpressions.Add(lambdaExpression);
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
