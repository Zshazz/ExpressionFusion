using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using ExtraConstraints;
using JetBrains.Annotations;

namespace ExpressionFusion
{
    public static class ExpressionFusor
    {
        /// <summary>
        /// Performs a "fusion" of two expressions. An inputExpr is used as a
        /// template/pattern for all usages of the argument of the planExpr.
        /// Effectively, it inlines all calls to inputExpr inside of planExpr.
        /// </summary>
        /// <remarks>
        /// This process could also be viewed as a form of partial application.
        /// </remarks>
        /// <typeparam name="TInputFunction">A delegate type</typeparam>
        /// <typeparam name="TOutputFunction">A delegate type</typeparam>
        /// <param name="inputExpr">The expression that will be inlined</param>
        /// <param name="planExpr">
        /// An expression that represents a function taking an expression. All
        /// occurrences of the input expression must be invocations and not
        /// usages or this process will fail. The intent is that any uses of
        /// the input function are effectively made inline inside the inner
        /// function.
        /// </param>
        /// <returns>
        /// The inside of the planExpr where all instances of the input
        /// expression are inlined.
        /// </returns>
        /// <exception cref="FusionDanglingReferenceException">
        /// Thrown when not all instances of the argument of planExpr are
        /// invocations (i.e. some non-inlinable usages of inputExpr still
        /// exist)
        /// </exception>
        public static Expression<TOutputFunction> FuseInto<[DelegateConstraint] TInputFunction, [DelegateConstraint] TOutputFunction>(
            [NotNull] this Expression<TInputFunction> inputExpr, [NotNull] Expression<Func<TInputFunction, TOutputFunction>> planExpr)
        {
            var planBodyAsLambda = (LambdaExpression) planExpr.Body;
            var visitedExpr = new FusorVisitor(inputExpr, planExpr).VisitAndConvert(planBodyAsLambda, "FuseInto");
            Debug.Assert(visitedExpr != null);
            return Expression.Lambda<TOutputFunction>(visitedExpr.Body, visitedExpr.Parameters);
        }

        private class FusorVisitor : ExpressionVisitor
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
                if(node.Expression != _planParam)
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

        private class ApplyParametersVisitor : ExpressionVisitor
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

    /// <summary>
    /// Exception returned when an expression fusion fails due to an uninvoked
    /// reference to the input expression.
    /// </summary>
    public class FusionDanglingReferenceException : Exception
    {
        internal FusionDanglingReferenceException([NotNull] string message)
            : base(message)
        {  }
    }
}
