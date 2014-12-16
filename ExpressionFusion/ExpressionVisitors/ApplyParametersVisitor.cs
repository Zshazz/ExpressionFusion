using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace ExpressionFusion.ExpressionVisitors
{
    internal class ApplyParametersVisitor : ExpressionVisitor
    {
        private readonly IDictionary<ParameterExpression, Expression> _paramsMapper;

        internal ApplyParametersVisitor([NotNull] LambdaExpression inputExpr, [NotNull] InvocationExpression targetExpr)
        {
            var paramsCount = inputExpr.Parameters.Count;
            Debug.Assert(paramsCount == inputExpr.Parameters.Count);
            _paramsMapper = new Dictionary<ParameterExpression, Expression>(paramsCount);
            for (var i = 0; i < paramsCount; ++i)
                _paramsMapper[inputExpr.Parameters[i]] = targetExpr.Arguments[i];

        }

        protected override Expression VisitParameter([NotNull] ParameterExpression node)
        {
            Expression target;
            if (_paramsMapper.TryGetValue(node, out target))
                return Visit(target);
            return base.VisitParameter(node);
        }
    }
}