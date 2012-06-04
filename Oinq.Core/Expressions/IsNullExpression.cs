﻿using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    internal class IsNullExpression : PigExpression
    {
        // constructors
        internal IsNullExpression(Expression expression)
            : base(PigExpressionType.IsNull, typeof(Boolean))
        {
            Expression = expression;
        }

        // internal properties
        internal Expression Expression { get; private set; }
    }
}
