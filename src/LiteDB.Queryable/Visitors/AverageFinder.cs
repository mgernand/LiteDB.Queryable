namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	internal sealed class AverageFinder : ExpressionVisitor
	{
		private LambdaExpression averageExpression;
		private readonly List<LambdaExpression> selectExpressions = new List<LambdaExpression>();

		public LambdaExpression GetAverageExpression(Expression expression)
		{
			this.Visit(expression);
			this.selectExpressions.Reverse();

			return this.averageExpression ?? this.selectExpressions.LastOrDefault();
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Average")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.averageExpression = lambdaExpression;
				}
			}

			if(node.Method.Name is "Select")
			{
				UnaryExpression expression = (UnaryExpression)node.Arguments[1];
				LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
				this.selectExpressions.Add(lambdaExpression);
			}

			this.Visit(node.Arguments[0]);

			return node;
		}
	}
}
