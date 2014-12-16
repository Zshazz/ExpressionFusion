using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace ExpressionFusion.ExpressionVisitors
{
    internal class FusorVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _planParam;
        private readonly LambdaExpression _inputExpr;

        internal FusorVisitor([NotNull] LambdaExpression inputExpr, [NotNull] LambdaExpression plan)
        {
            Debug.Assert(plan.Parameters.Count == 1 && inputExpr.Type == plan.Parameters[0].Type);
            _planParam = plan.Parameters[0];
            _inputExpr = inputExpr;
        }

        protected override Expression VisitInvocation([NotNull] InvocationExpression node)
        {
            if (node.Expression != _planParam)
                return base.VisitInvocation(node);
            return new ApplyParametersVisitor(_inputExpr, node).Visit(_inputExpr.Body);
        }

        protected override Expression VisitParameter([NotNull] ParameterExpression node)
        {
            if (node == _planParam)
                throw new FusionDanglingReferenceException(string.Format("All usages of {0} must be invocations.", _planParam.Name));
            return base.VisitParameter(node);
        }
    }
}