using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace ExpressionFusion.ExpressionVisitors
{
    internal class PartialApplyVisitor : ExpressionVisitor
    {
        private readonly IDictionary<ParameterExpression, LambdaExpression> _invocationDictionary;
        private readonly IDictionary<ParameterExpression, Expression> _otherDictionary; 

        internal PartialApplyVisitor([NotNull] LambdaExpression planExpr, ICollection<Expression> argsToApply)
        {
            Debug.Assert(planExpr.Parameters.Count >= argsToApply.Count);
            _invocationDictionary = new Dictionary<ParameterExpression, LambdaExpression>(argsToApply.Count);
            _otherDictionary = new Dictionary<ParameterExpression, Expression>(argsToApply.Count);

            foreach (var tup in planExpr.Parameters.Zip(argsToApply, Tuple.Create))
            {
                var planParam = tup.Item1;
                var argToApply = tup.Item2;

                Debug.Assert(planParam.Type == argToApply.Type);

                switch (argToApply.NodeType)
                {
                    case ExpressionType.Lambda:
                        var argAsLambda = (LambdaExpression)argToApply;
                        _invocationDictionary.Add(planParam, argAsLambda);
                        break;
                    default:
                        _otherDictionary.Add(planParam, argToApply);
                        break;
                }
            }
        }

        protected override Expression VisitInvocation([NotNull] InvocationExpression node)
        {
            // Try to inline the invocation if it's in the invocation dictionary
            LambdaExpression lambdaExpr;
            var nodeExprAsParam = node.Expression as ParameterExpression;
            if (nodeExprAsParam != null && _invocationDictionary.TryGetValue(nodeExprAsParam, out lambdaExpr))
                return new ApplyParametersVisitor(lambdaExpr, node).Visit(lambdaExpr.Body);

            return base.VisitInvocation(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_invocationDictionary.ContainsKey(node))
                throw new Exception(string.Format("All usages of {0} must be invocations.", node.Name));

            Expression expr;
            if (_otherDictionary.TryGetValue(node, out expr))
                return expr;

            return base.VisitParameter(node);
        }
    }
}