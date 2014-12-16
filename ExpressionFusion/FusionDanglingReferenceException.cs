using System;
using JetBrains.Annotations;

namespace ExpressionFusion
{
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