using BDO.GWT.Domain.Core.Retrievers;
using System;
using System.Linq.Expressions;

namespace BDO.GWT.Domain.Core;

/// <summary>
/// Provides helper methods for working with query expressions, including extracting property names and values from expressions and mapping expression types to query operators.
/// </summary>
public static class QueryExpressionHelper
{
    /// <summary>
    /// Gets the property name from an expression. This method supports BinaryExpression, MemberExpression, and UnaryExpression types. If the expression type is not supported, a NotSupportedException is thrown.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static string GetPropertyName(Expression expression)
    {
        var propertyName = expression switch
        {
            BinaryExpression binaryExpression => GetPropertyName(binaryExpression.Left),
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression => GetPropertyName(unaryExpression.Operand),
            _ => throw new NotSupportedException($"Getting property name from {expression.GetType().Name} is not supported"),
        };

        return propertyName;
    }

    /// <summary>
    /// Gets the corresponding QueryOperator for a given BinaryExpression. This method maps the ExpressionType of the binary expression to a QueryOperator. If the ExpressionType is not supported, a NotImplementedException is thrown.
    /// </summary>
    /// <param name="binaryExpression"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static QueryOperator GetQueryOperator(BinaryExpression binaryExpression)
    {
        var queryOperator = binaryExpression.NodeType switch
        {
            ExpressionType.Equal => QueryOperator.Equal,
            ExpressionType.GreaterThan => QueryOperator.GreaterThan,
            ExpressionType.GreaterThanOrEqual => QueryOperator.GreaterThanOrEqual,
            ExpressionType.LessThan => QueryOperator.LessThan,
            ExpressionType.LessThanOrEqual => QueryOperator.LessThanOrEqual,
            ExpressionType.NotEqual => QueryOperator.NotEqual,
            _ => throw new NotImplementedException($"Please add a map \"{binaryExpression.NodeType}\" -> QueryOperator."),
        };

        return queryOperator;
    }

    public static object? GetPropertyValue(Expression expression)
    {
        return Expression.Lambda(expression).Compile().DynamicInvoke();
    }
}
