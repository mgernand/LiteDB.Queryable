﻿namespace LiteDB.Queryable.Visitors
{
	using System.Linq.Expressions;

	internal sealed class MinFinder : ExpressionVisitor
	{
		private LambdaExpression sumExpression;

		public LambdaExpression GetMinExpression(Expression expression)
		{
			this.Visit(expression);
			return this.sumExpression;
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Min")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.sumExpression = lambdaExpression;
				}
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
