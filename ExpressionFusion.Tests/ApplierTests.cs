using System;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionFusion.Tests
{
    public class ApplierTests
    {
        [Fact]
        public void FullyPartiallyAppliedExpressionsWorkTheSameAsIfYouCalledThemNormally()
        {
            Expression<Func<int, Func<int>, int>> origExpr = (i, func) => i*func();

            var partialAppliedExpr = origExpr.Partial().Apply(3, (Expression<Func<int>>) (() => 5)).Result;

            Assert.Equal(origExpr.Compile()(3, () => 5), partialAppliedExpr.Compile()());
        }

        [Fact]
        public void SimplifiedFunctionsPerformIdenticallyToUnsimplifiedFunctions()
        {
            Expression<Func<Func<int>>> origExpr = () => () => 3 * 5;

            var simplifiedExpr = origExpr.Simplify();

            Assert.Equal(origExpr.Compile()()(), simplifiedExpr.Compile()());
        }

        [Fact]
        public void PartiallyAppliedExpressionsGiveYouWorkingExpressionsWithAppropriatelyFewerArguments()
        {
            Expression<Func<int, int, int>> origExpr = (i, j) => i + j;

            var partialAppliedExpr = origExpr.Partial().Apply(3).Result;

            Assert.Equal(origExpr.Compile()(3, 5), partialAppliedExpr.Compile()(5));
        }

        [Fact]
        public void PartiallyAppliedExpressionsGiveYouWorkingExpressionsWithAppropriatelyFewerArguments2()
        {
            Expression<Func<int, int, int, int>> origExpr = (i, j, h) => h*(i + j);

            var partialAppliedExpr = origExpr.Partial().Apply(3, 5).Result;

            Assert.Equal(origExpr.Compile()(3, 5, 7), partialAppliedExpr.Compile()(7));
        }
    }
}
