using System;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionFusion.Tests
{
    public class FuseIntoTests
    {
        [Fact]
        public void FusedExpressionsLookCorrect()
        {
            Expression<Func<int, int>> innerExpression = aNum => 5 + aNum;
            Expression<Func<Func<int, int>, Func<int, int>>> expr = expression => bNum => bNum * expression(bNum);
            Expression<Func<int, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<int, int>> desiredExpr = bNum => bNum*(5 + bNum);

            Assert.Equal(desiredExpr.ToString(), resultExpr.ToString());
        }

        [Fact]
        public void FusedExpressionsCanCompileAndWork()
        {
            Expression<Func<int, int>> innerExpression = aNum => 5 + aNum;
            Expression<Func<Func<int, int>, Func<int, int>>> expr = expression => bNum => bNum * expression(bNum);
            Expression<Func<int, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<int, int>> desiredExpr = bNum => bNum * (5 + bNum);

            var resultFunc = resultExpr.Compile();
            var desiredFunc = desiredExpr.Compile();

            Assert.Equal(desiredFunc(3), resultFunc(3));
            Assert.Equal(desiredFunc(5), resultFunc(5));
        }

        [Fact]
        public void FusionHasNoEffectIfNotUsed()
        {
            Expression<Func<int, int>> innerExpression = aNum => 5 + aNum;
            Expression<Func<Func<int, int>, Func<int, int>>> expr = expression => bNum => bNum * (5 + bNum);
            Expression<Func<int, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<int, int>> desiredExpr = bNum => bNum * (5 + bNum);

            var resultFunc = resultExpr.Compile();
            var desiredFunc = desiredExpr.Compile();

            Assert.Equal(desiredFunc(3), resultFunc(3));
            Assert.Equal(desiredFunc(5), resultFunc(5));
        }

        [Fact]
        public void FusionHasNoFunctionalEffectOnOtherInvocations()
        {
            Expression<Func<int, int>> innerExpression = aNum => 5 + aNum;
            Expression<Func<Func<int, int>, Func<Func<int>, int>>> expr = expression => bNumGetter => bNumGetter() * (5 + bNumGetter());
            Expression<Func<Func<int>, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<int, int>> simpleExpr = bNum => bNum * (5 + bNum);
            
            Assert.Equal(simpleExpr.Compile()(3), resultExpr.Compile()(() => 3));
        }

        [Fact]
        public void FusionHasNoVisibleEffectOnOtherInvocations()
        {
            Expression<Func<int, int>> innerExpression = aNum => 5 + aNum;
            Expression<Func<Func<int, int>, Func<Func<int>, int>>> expr = expression => bNumGetter => bNumGetter() * (5 + bNumGetter());
            Expression<Func<Func<int>, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<Func<int>, int>> desiredExpr = bNumGetter => bNumGetter() * (5 + bNumGetter());

            Assert.Equal(desiredExpr.ToString(), resultExpr.ToString());
        }

        [Fact]
        public void FusionCopiesArgumentsVerbatim()
        {
            Expression<Func<int, int>> innerExpression = aNum => aNum + aNum;
            Expression<Func<Func<int, int>, Func<int, int>>> expr = expression => bNum => bNum*expression(bNum);
            Expression<Func<int, int>> resultExpr = innerExpression.FuseInto(expr);
            Expression<Func<int, int>> desiredExpr = bNum => bNum*(bNum + bNum);

            Assert.Equal(desiredExpr.ToString(), resultExpr.ToString());
        }

        [Fact]
        public void FusionFailsWhenNotAllUsagesAreInvocations()
        {
            Expression<Func<int, int>> innerExpression = aNum => aNum + aNum;
            Expression<Func<Func<int, int>, Func<Func<int, int>>>> expr = inputExpr => () => inputExpr;

            Assert.Throws<FusionDanglingReferenceException>(() => innerExpression.FuseInto(expr).ToString());
        }
    }
}
