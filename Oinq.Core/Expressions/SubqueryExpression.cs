﻿using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    internal abstract class SubqueryExpression : PigExpression
    {
        // constructors
        internal SubqueryExpression(PigExpressionType eType, Type type, SelectExpression select)
            : base(eType, type)
        {
            Select = select;
        }

        // internal properties
        internal SelectExpression Select { get; private set; }
    }
}
