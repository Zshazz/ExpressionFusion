using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpressionFusion.ExpressionVisitors;
using JetBrains.Annotations;

namespace ExpressionFusion
{
    internal static class ApplierHelpers
    {
        internal static LambdaExpression Build(LambdaExpression input, IList<Expression> args)
        {
            if (!args.Any())
                return input;
            var newBody = new PartialApplyVisitor(input, args).Visit(input.Body);
            return Expression.Lambda(newBody, input.Parameters.Skip(args.Count));
        }
    }

    public static partial class Appliers
    {
        public static Expression<Func<TR>> Simplify<TR>([NotNull] this Expression<Func<Func<TR>>> expr)
        {
            var bodyAsLambda = (LambdaExpression)expr.Body;
            return Expression.Lambda<Func<TR>>(bodyAsLambda.Body, bodyAsLambda.Parameters);
        }
    }
}
