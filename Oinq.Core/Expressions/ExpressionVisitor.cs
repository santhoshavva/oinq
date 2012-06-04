﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// An abstract base class for an Expression visitor.
    /// </summary>
    internal abstract class ExpressionVisitor
    {
        // constructors
        /// <summary>
        /// Initializes a new _instance of the ExpressionVisitor class.
        /// </summary>
        protected ExpressionVisitor()
        {
        }

        // protected methods
        /// <summary>
        /// Visits an Expression.
        /// </summary>
        /// <param name="node">The Expression.</param>
        /// <returns>The Expression (posibly modified).</returns>
        protected virtual Expression Visit(Expression node)
        {
            if (node == null)
            {
                return node;
            }
            switch (node.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)node);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)node);
                case ExpressionType.TypeIs:
                    return VisitTypeBinary((TypeBinaryExpression)node);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)node);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)node);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)node);
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)node);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)node);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)node);
                case ExpressionType.New:
                    return VisitNew((NewExpression)node);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)node);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)node);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)node);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)node);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", node.NodeType));
            }
        }

        /// <summary>
        /// Visits an Expression list.
        /// </summary>
        /// <param name="nodes">The Expression list.</param>
        /// <returns>The Expression list (possibly modified).</returns>
        protected ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes)
        {
            if (nodes != null)
            {
                List<Expression> list = null;
                for (Int32 i = 0, n = nodes.Count; i < n; i++)
                {
                    Expression node = Visit(nodes[i]);
                    if (list != null)
                    {
                        list.Add(node);
                    }
                    else if (node != nodes[i])
                    {
                        list = new List<Expression>(n);
                        for (Int32 j = 0; j < i; j++)
                        {
                            list.Add(nodes[j]);
                        }
                        list.Add(node);
                    }
                }
                if (list != null)
                {
                    return list.AsReadOnly();
                }
            }
            return nodes;
        }

        /// <summary>
        /// Visits a BinaryExpression.
        /// </summary>
        /// <param name="node">The BinaryExpression.</param>
        /// <returns>The BinaryExpression (possibly modified).</returns>
        protected virtual Expression VisitBinary(BinaryExpression node)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            Expression conversion = Visit(node.Conversion);
            if (left != node.Left || right != node.Right || conversion != node.Conversion)
            {
                if (node.NodeType == ExpressionType.Coalesce && node.Conversion != null)
                {
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
                }
            }
            return node;
        }

        /// <summary>
        /// Visits a ConditionalExpression.
        /// </summary>
        /// <param name="node">The ConditionalExpression.</param>
        /// <returns>The ConditionalExpression (possibly modified).</returns>
        protected virtual Expression VisitConditional(ConditionalExpression node)
        {
            Expression test = Visit(node.Test);
            Expression ifTrue = Visit(node.IfTrue);
            Expression ifFalse = Visit(node.IfFalse);
            if (test != node.Test || ifTrue != node.IfTrue || ifFalse != node.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return node;
        }

        /// <summary>
        /// Visits a ConstantExpression.
        /// </summary>
        /// <param name="node">The ConstantExpression.</param>
        /// <returns>The ConstantExpression (possibly modified).</returns>
        protected virtual Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits an ElementInit.
        /// </summary>
        /// <param name="node">The ElementInit.</param>
        /// <returns>The ElementInit (possibly modified).</returns>
        protected virtual ElementInit VisitElementInit(ElementInit node)
        {
            ReadOnlyCollection<Expression> arguments = Visit(node.Arguments);
            if (arguments != node.Arguments)
            {
                return Expression.ElementInit(node.AddMethod, arguments);
            }
            return node;
        }

        /// <summary>
        /// Visits an ElementInit list.
        /// </summary>
        /// <param name="nodes">The ElementInit list.</param>
        /// <returns>The ElementInit list (possibly modified).</returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitList(
            ReadOnlyCollection<ElementInit> nodes)
        {
            List<ElementInit> list = null;
            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                ElementInit node = VisitElementInit(nodes[i]);
                if (list != null)
                {
                    list.Add(node);
                }
                else if (node != nodes[i])
                {
                    list = new List<ElementInit>(n);
                    for (Int32 j = 0; j < i; j++)
                    {
                        list.Add(nodes[j]);
                    }
                    list.Add(node);
                }
            }
            if (list != null)
            {
                return list;
            }
            return nodes;
        }

        /// <summary>
        /// Visits an InvocationExpression.
        /// </summary>
        /// <param name="node">The InvocationExpression.</param>
        /// <returns>The InvocationExpression (possibly modified).</returns>
        protected virtual Expression VisitInvocation(InvocationExpression node)
        {
            IEnumerable<Expression> args = Visit(node.Arguments);
            Expression expr = Visit(node.Expression);
            if (args != node.Arguments || expr != node.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return node;
        }

        /// <summary>
        /// Visits a LambdaExpression.
        /// </summary>
        /// <param name="node">The LambdaExpression.</param>
        /// <returns>The LambdaExpression (possibly modified).</returns>
        protected virtual Expression VisitLambda(LambdaExpression node)
        {
            Expression body = Visit(node.Body);
            if (body != node.Body)
            {
                return Expression.Lambda(node.Type, body, node.Parameters);
            }
            return node;
        }

        /// <summary>
        /// Visits a ListInitExpression.
        /// </summary>
        /// <param name="node">The ListInitExpression.</param>
        /// <returns>The ListInitExpression (possibly modified).</returns>
        protected virtual Expression VisitListInit(ListInitExpression node)
        {
            NewExpression n = VisitNew(node.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitList(node.Initializers);
            if (n != node.NewExpression || initializers != node.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return node;
        }

        /// <summary>
        /// Visits a MemberExpression.
        /// </summary>
        /// <param name="node">The MemberExpression.</param>
        /// <returns>The MemberExpression (possibly modified).</returns>
        protected virtual Expression VisitMember(MemberExpression node)
        {
            Expression exp = Visit(node.Expression);
            if (exp != node.Expression)
            {
                return Expression.MakeMemberAccess(exp, node.Member);
            }
            return node;
        }

        /// <summary>
        /// Visits a MemberAssignment.
        /// </summary>
        /// <param name="node">The MemberAssignment.</param>
        /// <returns>The MemberAssignment (possibly modified).</returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            Expression e = Visit(node.Expression);
            if (e != node.Expression)
            {
                return Expression.Bind(node.Member, e);
            }
            return node;
        }

        /// <summary>
        /// Visits a MemberBinding.
        /// </summary>
        /// <param name="node">The MemberBinding.</param>
        /// <returns>The MemberBinding (possibly modified).</returns>
        protected virtual MemberBinding VisitMemberBinding(MemberBinding node)
        {
            switch (node.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)node);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)node);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)node);
                default:
                    throw new Exception(String.Format("Unhandled binding type '{0}'", node.BindingType));
            }
        }

        /// <summary>
        /// Visits a MemberBinding list.
        /// </summary>
        /// <param name="nodes">The MemberBinding list.</param>
        /// <returns>The MemberBinding list (possibly modified).</returns>
        protected virtual IEnumerable<MemberBinding> VisitMemberBindingList(ReadOnlyCollection<MemberBinding> nodes)
        {
            List<MemberBinding> list = null;
            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                MemberBinding node = VisitMemberBinding(nodes[i]);
                if (list != null)
                {
                    list.Add(node);
                }
                else if (node != nodes[i])
                {
                    list = new List<MemberBinding>(n);
                    for (Int32 j = 0; j < i; j++)
                    {
                        list.Add(nodes[j]);
                    }
                    list.Add(node);
                }
            }
            if (list != null)
            {
                return list;
            }
            return nodes;
        }

        /// <summary>
        /// Visits a MemberInitExpression.
        /// </summary>
        /// <param name="node">The MemberInitExpression.</param>
        /// <returns>The MemberInitExpression (possibly modified).</returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression node)
        {
            NewExpression n = VisitNew(node.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitMemberBindingList(node.Bindings);
            if (n != node.NewExpression || bindings != node.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return node;
        }

        /// <summary>
        /// Visits a MemberListBinding.
        /// </summary>
        /// <param name="node">The MemberListBinding.</param>
        /// <returns>The MemberListBinding (possibly modified).</returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitList(node.Initializers);
            if (initializers != node.Initializers)
            {
                return Expression.ListBind(node.Member, initializers);
            }
            return node;
        }

        /// <summary>
        /// Visits a MemberMemberBinding.
        /// </summary>
        /// <param name="node">The MemberMemberBinding.</param>
        /// <returns>The MemberMemberBinding (possibly modified).</returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            IEnumerable<MemberBinding> bindings = VisitMemberBindingList(node.Bindings);
            if (bindings != node.Bindings)
            {
                return Expression.MemberBind(node.Member, bindings);
            }
            return node;
        }

        /// <summary>
        /// Visits a MethodCallExpression.
        /// </summary>
        /// <param name="node">The MethodCallExpression.</param>
        /// <returns>The MethodCallExpression (possibly modified).</returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression obj = Visit(node.Object);
            IEnumerable<Expression> args = Visit(node.Arguments);
            if (obj != node.Object || args != node.Arguments)
            {
                return Expression.Call(obj, node.Method, args);
            }
            return node;
        }

        /// <summary>
        /// Visits a NewExpression.
        /// </summary>
        /// <param name="node">The NewExpression.</param>
        /// <returns>The NewExpression (possibly modified).</returns>
        protected virtual NewExpression VisitNew(NewExpression node)
        {
            IEnumerable<Expression> args = Visit(node.Arguments);
            if (args != node.Arguments)
            {
                if (node.Members != null)
                {
                    return Expression.New(node.Constructor, args, node.Members);
                }
                else
                {
                    return Expression.New(node.Constructor, args);
                }
            }
            return node;
        }

        /// <summary>
        /// Visits a NewArrayExpression.
        /// </summary>
        /// <param name="node">The NewArrayExpression.</param>
        /// <returns>The NewArrayExpression (possibly modified).</returns>
        protected virtual Expression VisitNewArray(NewArrayExpression node)
        {
            IEnumerable<Expression> exprs = Visit(node.Expressions);
            if (exprs != node.Expressions)
            {
                if (node.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(node.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(node.Type.GetElementType(), exprs);
                }
            }
            return node;
        }

        /// <summary>
        /// Visits a ParameterExpression.
        /// </summary>
        /// <param name="node">The ParameterExpression.</param>
        /// <returns>The ParameterExpression (possibly modified).</returns>
        protected virtual Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits a TypeBinaryExpression.
        /// </summary>
        /// <param name="node">The TypeBinaryExpression.</param>
        /// <returns>The TypeBinaryExpression (possibly modified).</returns>
        protected virtual Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Expression expr = Visit(node.Expression);
            if (expr != node.Expression)
            {
                return Expression.TypeIs(expr, node.TypeOperand);
            }
            return node;
        }

        /// <summary>
        /// Visits a UnaryExpression.
        /// </summary>
        /// <param name="node">The UnaryExpression.</param>
        /// <returns>The UnaryExpression (possibly modified).</returns>
        protected virtual Expression VisitUnary(UnaryExpression node)
        {
            Expression operand = Visit(node.Operand);
            if (operand != node.Operand)
            {
                return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
            }
            return node;
        }
    }
}
