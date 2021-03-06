﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Interface for accessing Pig query text.
    /// </summary>
    public interface IPigQueryable
    {
        /// <summary>
        /// Gets the Pig query text.
        /// </summary>
        /// <returns></returns>
        String GetPigQuery();
    }

    /// <summary>
    /// An implementation IQueryable{{T}} for querying a data file.
    /// </summary>
    /// <typeparam name="T">The type of facts being queried.</typeparam>
    public sealed class Query<T> : IOrderedQueryable<T>, IPigQueryable
    {
        // private fields
        private readonly Expression _expression;
        private readonly IQueryProvider _provider;

        // constructors
        /// <summary>
        /// Initializes a new member of the Query class.
        /// </summary>
        /// <param name="provider">The query provider.</param>
        public Query(IQueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            _provider = provider;
            _expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a new member of the Query class.
        /// </summary>
        /// <param name="provider">The query provider.</param>
        /// <param name="expression">The expression.</param>
        public Query(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            _provider = provider;
            _expression = expression;
        }

        // public methods

        #region IPigQueryable Members

        /// <summary>
        /// Gets the Pig query that will be sent to the server when this LINQ query is executed.
        /// </summary>
        /// <returns>The Pig query.</returns>
        public String GetPigQuery()
        {
            return ((QueryProvider) _provider).BuildQueryText(this);
        }

        #endregion

        #region IQueryable<T> Members

        /// <summary>
        /// Gets an _enumerator for the results of an Pig LINQ query.
        /// </summary>
        /// <returns>An _enumerator for the results of an Pig LINQ query.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) _provider.Execute(_expression)).GetEnumerator();
        }

        // explicit implementation of IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _provider.Execute(_expression)).GetEnumerator();
        }

        // explicit implementation of IQueryable
        Type IQueryable.ElementType
        {
            get { return typeof (T); }
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        #endregion

        /// <summary>
        /// Returns a textual representation of the Query.
        /// </summary>
        /// <returns>The String.</returns>
        public override String ToString()
        {
            if (_expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression) _expression).Value == this)
            {
                return "Query(" + typeof (T) + ")";
            }
            return _expression.ToString();
        }
    }
}