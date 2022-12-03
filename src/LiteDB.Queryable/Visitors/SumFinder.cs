namespace LiteDB.Queryable.Visitors
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;

	internal sealed class SumFinder : ExpressionVisitor
	{
		private LambdaExpression sumExpression;
		private readonly List<LambdaExpression> selectExpressions = new List<LambdaExpression>();

		public LambdaExpression GetSumExpression(Expression expression)
		{
			this.Visit(expression);
			this.selectExpressions.Reverse();

			return this.sumExpression ?? this.selectExpressions.LastOrDefault();
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if(node.Method.Name is "Sum")
			{
				if(node.Arguments.Count == 2)
				{
					UnaryExpression expression = (UnaryExpression)node.Arguments[1];
					LambdaExpression lambdaExpression = (LambdaExpression)expression.Operand;
					this.sumExpression = lambdaExpression;
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
