﻿using System;

namespace Oinq.Expressions
{
    internal class ScalarExpression : SubqueryExpression
    {
        internal ScalarExpression(Type type, SelectExpression select)
            : base(PigExpressionType.Scalar, type, select)
        {
        }
    }
}