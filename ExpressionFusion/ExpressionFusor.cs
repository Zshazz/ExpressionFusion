using System;
using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionFusion.ExpressionVisitors;
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
    }
}
